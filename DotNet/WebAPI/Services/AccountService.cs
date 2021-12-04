using C200.WebApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace C200.WebApi.Services;

public interface IAccountService
{
   SysUser? Authenticate(Login credential);
   SysUser? GetById(string id);
   IEnumerable<SysUser> GetAll();
}

public class AccountService : IAccountService
{
   private static readonly string? env = 
      Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
   private static readonly IConfiguration config =
      new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile("appsettings.json")
         .AddJsonFile($"appsettings.{env}.json")
         .Build()
         .GetSection("EncryptSettings");

   private static readonly string? SECRET = config.GetValue<String>("SecretKey");

   private static readonly IDBService _dbs = new DBService();

   private const string LOGIN_SQL =
      @"SELECT * FROM SysUser 
         WHERE UserId = '{0}' 
           AND UserPw = HASHBYTES('SHA1', CONVERT(VARCHAR, '{1}'))";

   private const string LASTLOGIN_SQL =
      @"UPDATE SysUser SET LastLogin=GETDATE() WHERE UserId='{0}'";

   // public SysUser? Authenticate(string uid, string upw)
   public SysUser? Authenticate(Login credential)
   {
      List<SysUser> list = 
         _dbs.GetList<SysUser>(LOGIN_SQL, credential.UserId, credential.Password);
      if (list.Count != 1)
         return null;

      // Update the Last Login Timestamp of the User
      _dbs.ExecSQL(LASTLOGIN_SQL, credential.UserId);

      var user = list[0];

      // Generate JWT Token
      var tokenHandler = new JwtSecurityTokenHandler();
      var key = Encoding.ASCII.GetBytes(SECRET ?? "SIGN_&_VERIFY_JWT_TOKENS");
      var tokenDescriptor = new SecurityTokenDescriptor
      {
         Subject =
            new ClaimsIdentity(
               new Claim[]
               {
                    new Claim(ClaimTypes.Name, user.UserId),
                    new Claim(ClaimTypes.Role, user.UserRole)
               }),
         Expires = DateTime.UtcNow.AddDays(7),
         // Expires= DateTime.UtcNow.AddMinutes(120),
         SigningCredentials =
            new SigningCredentials(
               new SymmetricSecurityKey(key),
               SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken(tokenDescriptor);
      user.Token = tokenHandler.WriteToken(token);

      return user;
   }

   public IEnumerable<SysUser> GetAll()
   {
      return _dbs.GetList<SysUser>("SELECT * FROM SysUser");
   }

   public SysUser? GetById(string userid)
   {
      List<SysUser> list = _dbs.GetList<SysUser>(
         "SELECT * FROM SysUser WHERE UserId='{0}'", 
         userid);
      if (list.Count == 1)
         return list[0];
      else
         return null;
   }
}

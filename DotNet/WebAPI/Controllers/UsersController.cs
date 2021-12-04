using C200.WebApi.Models;
using C200.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;


// http://jasonwatmore.com/post/2019/01/08/aspnet-core-22-role-based-authorization-tutorial-with-example-api#users-controller-cs

// https://www.c-sharpcorner.com/article/authentication-authorization-using-net-core-web-api-using-jwt-token-and/
namespace C200.WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("api/[controller]")]
   public class UsersController : ControllerBase
   {
      private readonly IAccountService _accSvc;
      private readonly IDBService _sbSvc;

      public UsersController(IAccountService accountService,
                             IDBService dbService)
      {
         _accSvc = accountService;
         _sbSvc = dbService;
      }

      [AllowAnonymous]
      [HttpPost("Authenticate")]
      public IActionResult Authenticate([FromBody]Login credential)
      {
         var user = _accSvc.Authenticate(credential);

         if (user == null)
            return BadRequest(new { message = "Username or password is incorrect" });
         
         return Ok(user.Token);
      }

      [Authorize(Roles = "manager")]
      [HttpGet("Roles")]
      public ActionResult<IEnumerable<string>> Get()
      {
         DataTable dt = _sbSvc.GetTable("SELECT DISTINCT UserRole FROM SysUser");
         List<String> result = dt
            .AsEnumerable()
            .Select(row => new String((String)row["UserRole"]))
            .ToList();

         return result;
      }

      [Authorize(Roles = "admin")]
      [HttpGet("Users")]
      public IActionResult GetAll()
      {
         var users = 
            _accSvc
               .GetAll()
               .Select(
                  user => new {user.UserId, user.FullName, user.Email, user.UserRole})
               .ToList();
        
         return Ok(users);
      }

      [Authorize]
      [HttpGet("{id}")]
      public IActionResult GetById(string id)
      {
         var user = _accSvc.GetById(id);
         if (user == null)
         {
            return NotFound();
         }
         
         return Ok(new {user.UserId, user.FullName, user.Email, user.UserRole});
      }
   }
}

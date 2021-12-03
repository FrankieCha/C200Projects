using C200.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

IConfiguration config =
    new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json")
       .Build()
       .GetSection("EncryptSettings");

string SECRETKEY = config.GetValue<String>("SecretKey");

var builder = WebApplication.CreateBuilder(args);

//
// Add services to the container.
//
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(x => {
                                            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                                        })
                .AddJwtBearer(x => {
                                      x.RequireHttpsMetadata = false;
                                      x.SaveToken = true;
                                      x.TokenValidationParameters = 
                                          new TokenValidationParameters
                                          {
                                             ValidateIssuerSigningKey = true,
                                             IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SECRETKEY)),
                                             ValidateIssuer = false,
                                             ValidateAudience = false
                                          };
                                    });

//
// configure DI for application services
//
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IDBService, DBService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseSwagger();
   app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

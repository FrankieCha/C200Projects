using C200.WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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
//builder.Services.AddControllers();

builder.Services.AddControllersWithViews();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
   swg => {
            swg.AddSecurityDefinition(
               "Bearer", 
               new OpenApiSecurityScheme()
               {
                  Name = "Authorization",
                  Type = SecuritySchemeType.ApiKey,
                  Scheme = "Bearer",
                  BearerFormat = "JWT",
                  In = ParameterLocation.Header,
                  Description =
                     "JWT Authorization Header using the Bearer Scheme.\r\n\r\n" +
                     "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                     "Example: \"Bearer 12345_TOKEN_abcdef\"\r\n\r\n ",
               });
            swg.AddSecurityRequirement(
               new OpenApiSecurityRequirement
               {
                  {
                     new OpenApiSecurityScheme
                     {
                        Reference = new OpenApiReference
                        {
                           Type = ReferenceType.SecurityScheme,
                           Id = "Bearer"
                        }
                     },
                     Array.Empty<string>()
                  }
               });
   });
builder.Services.AddAuthentication(
   opt => {
             opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
             opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          })
                .AddJwtBearer(
   opt => {
             opt.RequireHttpsMetadata = false;
             opt.SaveToken = true;
             opt.TokenValidationParameters = 
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

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

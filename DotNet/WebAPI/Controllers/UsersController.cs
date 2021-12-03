using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using C200.WebApi.Models;
using C200.WebApi.Services;


// http://jasonwatmore.com/post/2019/01/08/aspnet-core-22-role-based-authorization-tutorial-with-example-api#users-controller-cs
namespace C200.WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("api/[controller]")]
   public class UsersController : ControllerBase
   {
      private IAccountService _usvc;
      private IDBService _dbsvc;

      public UsersController(IAccountService userService,
                             IDBService dbService)
      {
         _usvc = userService;
         _dbsvc = dbService;
      }

      [AllowAnonymous]
      [HttpGet("xyz")]
      public ActionResult<IEnumerable<string>> Get()
      {
         DataTable dt = _dbsvc.GetTable("SELECT DISTINCT UserRole FROM SysUser");
         List<String> result = dt
            .AsEnumerable()
            .Select(row => new String((String)row["UserRole"]))
            .ToList();

         return result;
      }

      [AllowAnonymous]
      [HttpPost("authenticate")]
      public IActionResult Authenticate([FromBody]SysUser userParam)
      {
         var user = _usvc.Authenticate(userParam.UserId, userParam.Password);

         if (user == null)
            return BadRequest(new { message = "Username or password is incorrect" });

         return Ok(user.Token);
      }

      [Authorize(Roles = "admin")]
      [HttpGet("getall")]
      public IActionResult GetAll()
      {
         var users = _usvc.GetAll();
         return Ok(users);
      }

      [HttpGet("{id}")]
      public IActionResult GetById(string id)
      {
         var user = _usvc.GetById(id);

         if (user == null)
         {
            return NotFound();
         }

         // only allow admins to access other user records
         //var currentUserId = int.Parse(User.Identity.Name);
         //if (id != currentUserId && !User.IsInRole(Role.Admin))
         //{
         //   return Forbid();
         //}

         return Ok(user);
      }
   }
}

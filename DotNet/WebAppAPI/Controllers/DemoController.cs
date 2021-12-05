using C200.WebAppAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace C200.WebAppAPI.Controllers;

public class DemoController : Controller
{

   private readonly IConfiguration _config;

   public DemoController(IConfiguration Configuration)
   {
      _config = Configuration;
   }

   public async Task<IActionResult> Heroes()
   {
      var list = await GrabData();
      return await Task.Run(() => View(list));
   }

   public async Task<IActionResult> Index()
   {
      var endpoint = _config["WebAPI:EndPoint"] + "/users/authenticate";

      using (var client = new HttpClient())
      {
         var json = "{'UserId' : 'mary', 'Password' : 'secret0' }".Replace("'", "\"");
         var response = 
            await client.PostAsync(endpoint, 
                                   new StringContent(json, Encoding.UTF8, "application/json"));

         String token = await response.Content.ReadAsStringAsync();
         HttpContext.Session.SetString("JWT", token);
      }
      return await Task.Run(() => RedirectToAction("Heroes"));
   }

   private async Task<List<DataRecord>> GrabData()
   {
      var endpoint = _config["WebAPI:EndPoint"] + "/metahuman/getall";

      string accessToken = HttpContext.Session.GetString("JWT") ?? "";
      
      using var client = new HttpClient();
      client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
      var responses = 
         await client.GetStringAsync(endpoint);
      
      List<DataRecord>? heroes = JsonConvert.DeserializeObject<List<DataRecord>>(responses);
      return heroes ?? new List<DataRecord>();
   }



}

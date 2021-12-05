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

   public async Task<IActionResult> Index()
   {
      var list = await GrabData();
      return await Task.Run(() => View(list));
   }

   public async Task<IActionResult> Test()
   {

      var endpoint = _config["WebAPI:EndPoint"] + "/users/authenticate";
      // var endpoint = "https://localhost:44343/api/users/authenticate";
      using (var client = new HttpClient())
      {
         //client.DefaultRequestHeaders.Add("UserId", "mary");
         //client.DefaultRequestHeaders.Add("Password", "secret0");


         var json = "{'UserId' : 'mary', 'Password' : 'secret0' }";

         var response = await client.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));

         //dynamic obj = response.Content.ReadAsAsync<ExpandoObject>().Result;
         //HttpContext.Session.SetString("JWT", (String)obj);

         String token = await response.Content.ReadAsStringAsync();
         HttpContext.Session.SetString("JWT", token);

      }
      return await Task.Run(() => RedirectToAction("Heroes"));
   }

   private async Task<List<DataRecord>> GrabData()
   {
      var endpoint = "https://localhost:44343/api/demo";
      string accessToken = HttpContext.Session.GetString("JWT");
      using (var client = new HttpClient())
      {
         //  HttpClient client = new HttpClient();
         client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
         var responses = await client.GetStringAsync(endpoint);
         List<DataRecord> room = JsonConvert.DeserializeObject<List<DataRecord>>(responses);
         return room;
      }
   }



}

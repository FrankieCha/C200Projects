using Microsoft.AspNetCore.Mvc;

namespace C200.WebAppAPI.Controllers;

[ApiExplorerSettings(IgnoreApi = true)] // Suppress Swagger

[ApiController]
[Route("api/[controller]")]
public class ValuesController : ControllerBase
{
   // GET api/values
   [HttpGet]
   public ActionResult<IEnumerable<string>> Get()
   {
      return new string[] { "v1", "v2" };
   }

   // GET api/values/5
   [HttpGet("{id}")]
   public ActionResult<string> Get(int id)
   {
      return "value";
   }

   // POST api/values
   [HttpPost]
   public void Post([FromBody] string value)
   {
   }

   // PUT api/values/5
   [HttpPut("{id}")]
   public void Put(int id, [FromBody] string value)
   {
   }

   // DELETE api/values/5
   [HttpDelete("{id}")]
   public void Delete(int id)
   {
   }
}


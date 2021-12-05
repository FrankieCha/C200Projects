using C200.WebAppAPI.Models;
using C200.WebAppAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace C200.WebAppAPI.Controllers;

// [Authorize]
[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class MetaHumanController : Controller
{
   private readonly IDBService _dbSvc;
   private readonly IWebHostEnvironment _env;

   public MetaHumanController(IDBService dbService,
                              IWebHostEnvironment env)
   {
      _dbSvc = dbService;
      _env = env;
   }

   //
   // GET api/MetaHuman
   //
   [HttpGet("GetAll")]
   public IEnumerable<DataRecord> Get()
   {
      List<DataRecord> dbList = _dbSvc.GetList<DataRecord>("SELECT * FROM DataRecord");
      return dbList;
   }

   //
   // GET api/MetaHuman/batman
   //
   [HttpGet("{id}")]
   public IActionResult Get(string id)
   {
      List<DataRecord> dbList = _dbSvc.GetList<DataRecord>($"SELECT * FROM DataRecord WHERE Field0='{id}'");
      if (dbList.Count > 0)
         return Ok(dbList[0]);
      else
         return NotFound();
   }

   //
   // POST api/MetaHuman
   //
   [HttpPost]
   public IActionResult Post([FromBody]DataRecord rec)
   {
      if (rec == null)
      {
         return BadRequest();
      }

      string sql = @"INSERT INTO DataRecord(Field1,Field2,Field3,Field4,Field5,Field6) VALUES
                     ('{0}','{1}',{2},{3},'{4}','{5}');";
      if (_dbSvc.ExecSQL(sql, 
                           rec.Field1, rec.Field2, rec.Field3,
                           rec.Field4, rec.Field5, rec.Field6.ToUniversalTime().ToString("s")) == 1)
         return Ok();
      else
         return BadRequest(new { Message = "Insert Failed" });
   }

   // PUT api/MetaHuman/superman
   //[Authorize(Roles = "manager")]
   [HttpPut("{id}")]
   public IActionResult Put(string id, [FromBody]DataRecord rec)
   {
      if (rec == null || id == null)
      {
         return BadRequest();
      }

      string sql = @"UPDATE DataRecord 
                        SET Field1 = '{1}',
                              Field2 = '{2}',
                              Field3 = {3},
                              Field4 = {4},
                              Field5 = '{5}',
                              Field6 = '{6}'
                        WHERE Field0 = {0}";
      if (_dbSvc.ExecSQL(sql, id,
                           rec.Field1, rec.Field2, 
                           rec.Field3, rec.Field4, 
                           rec.Field5, rec.Field6.ToUniversalTime().ToString("s")) == 1)
         return Ok();
      else
         return BadRequest(new { Message = "Update Failed" });
   }

   // DELETE api/MetaHuman/7
   //[Authorize(Roles = "admin")]
   [HttpDelete("{id}")]
   public IActionResult Delete(string id)
   {
      if (id == null)
      {
         return BadRequest();
      }

      string sql = @"DELETE DataRecord 
                        WHERE Field0={0}";
      string delete = String.Format(sql, id);
      if (_dbSvc.ExecSQL(delete) == 1)
         return Ok();
      else
         return BadRequest(new { Message = "Delete Failed" });

   }

   [HttpPost("{id}/Picture")]
   public async Task<IActionResult> UploadPicture([FromRoute]string id, IFormFile photo)
   {
      string fext = Path.GetExtension(photo.FileName);
      string uname = Guid.NewGuid().ToString();
      string fname = id + "_" + uname + fext;

      string path = Path.Combine(_env.WebRootPath, "profile\\" + fname);
      using (var stream = new FileStream(path, FileMode.Create))
      {
         await photo.CopyToAsync(stream);
         stream.Close();
      }
      return Ok(fname);
   }




}


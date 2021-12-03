using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using C200.WebApi.Models;
using C200.WebApi.Services;

namespace C200.WebApi.Controllers
{
   [Authorize]
   [ApiController]
   [Route("api/[controller]")]
   public class DemoController : Controller
   {
      private IDBService _dbsvc;

      public DemoController(IDBService dbService)
      {
         _dbsvc = dbService;
      }

      // GET api/demo
      [Authorize(Roles ="admin")]
      [HttpGet]
      public IEnumerable<DataRecord> Get()
      {
         List<DataRecord> dbList = _dbsvc.GetList<DataRecord>("SELECT * FROM DataRecord");
         return dbList;
      }

      // GET api/demo/batman
      [HttpGet("{id}")]
      public IActionResult Get(string id)
      {
         List<DataRecord> dbList = _dbsvc.GetList<DataRecord>($"SELECT * FROM DataRecord WHERE Field1='{id}'");
         if (dbList.Count >0)
            return Ok(dbList[0]);
         else
            return NotFound();
      }

      // POST api/demo
      [HttpPost]
      public IActionResult Post([FromBody]DataRecord rec)
      {
         if (rec == null)
         {
            return BadRequest();
         }

         string sql = @"INSERT INTO DataRecord(Field1,Field2,Field3,Field4,Field5,Field6) VALUES
                        ('{0}','{1}',{2},{3},'{4}','{5}');";
         string insert = String.Format(sql, rec.Field1, rec.Field2, rec.Field3,
                                       rec.Field4, rec.Field5, rec.Field6.ToUniversalTime().ToString("s"));
         if (_dbsvc.ExecSQL(insert) == 1)
            return Ok();
         else
            return BadRequest(new { Message = _dbsvc.GetLastErrMsg() });
      }

      // PUT api/demo/superman
      [HttpPut("{id}")]
      public IActionResult Put(string id, [FromBody]DataRecord rec)
      {
         if (rec == null || id == null)
         {
            return BadRequest();
         }

         string sql = @"UPDATE DataRecord 
                           SET Field2 = '{1}',
                               Field3 = {2},
                               Field4 = {3},
                               Field5 = '{4}',
                               Field6 = '{5}'
                         WHERE Field1='{0}'";
         string update = String.Format(sql, rec.Field1, rec.Field2, rec.Field3,
                                       rec.Field4, rec.Field5, rec.Field6.ToUniversalTime().ToString("s"));
         if (_dbsvc.ExecSQL(update) == 1)
            return Ok();
         else
            return BadRequest(new { Message = _dbsvc.GetLastErrMsg() });
      }

      // DELETE api/demo/wonderwoman
      [HttpDelete("{id}")]
      public IActionResult Delete(string id)
      {
         if (id == null)
         {
            return BadRequest();
         }

         string sql = @"DELETE DataRecord 
                         WHERE Field1='{0}'";
         string delete = String.Format(sql, id);
         if (_dbsvc.ExecSQL(delete) == 1)
            return Ok();
         else
            return BadRequest(new { Message = _dbsvc.GetLastErrMsg() });

      }
   }
}

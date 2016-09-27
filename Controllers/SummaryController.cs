using Excavator.Models.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Excavator.Controllers {
  public class SummaryController : ApiController {
    private MongoSearchContext ctx = new MongoSearchContext();

    public IHttpActionResult Get() {
      var domains = ctx.Domains.OrderBy(x => x.Name);
      
      return Ok(domains);
    }

    // GET api/values/5
    public string Get(int id) {
      return "value";
    }

    // POST api/values
    public void Post([FromBody]string value) {
    }

    // PUT api/values/5
    public void Put(int id, [FromBody]string value) {
    }

    // DELETE api/values/5
    public void Delete(int id) {
    }
  }
}

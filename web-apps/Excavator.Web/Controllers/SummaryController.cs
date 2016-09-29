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
      var domains = ctx.Domains.Select(x => new { x.Name, x.Documents }).OrderBy(x => x.Name);
      
      return Ok(domains);
    }
  }
}

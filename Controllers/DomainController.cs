using Excavator.Models.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Excavator.Controllers {
  public class DomainController : ApiController {
    private MongoSearchContext ctx = new MongoSearchContext();

    [Route("api/domain/add"), HttpGet]
    public async Task<IHttpActionResult> AddDomain(string url) {
      if (string.IsNullOrEmpty(url)) { return BadRequest(); }

      var uri = new Uri(url);
      var Domain = new Domain { Documents = 0, Name = uri.Host, RobotTexts = RobotsText.ProcessRobotFile(url) };

      ctx.Domains.Add(Domain);

      return Ok(Domain);
    }
  }
}
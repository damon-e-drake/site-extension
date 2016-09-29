using Excavator.Models.Mongo;
using Excavator.Models.SearchEngine;
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
      var Domain = new Domain { Documents = 1, Name = uri.Host, RobotTexts = RobotsText.ProcessRobotFile(url) };
      var Document = new Document { Host = uri.Host, Url = url, Indexed = false, LastIndexed = DateTime.Now };
      ctx.Domains.Add(Domain);
      ctx.Documents.Add(Document);

      return Ok(Domain);
    }

    [Route("api/domain/index"), HttpGet]
    public async Task<IHttpActionResult> IndexDomain(string host) {
      var crawler = new WebCrawler(host);
      return Ok(await crawler.CrawlPages());
    }
  }
}
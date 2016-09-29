using Excavator.Models.Mongo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Excavator.Models.SearchEngine {
  public class WebCrawler {
    private static string _title;
    private static string _version;
    private static int _queue;
    private static HttpClient _client;
    private static MongoSearchContext context = new MongoSearchContext();
    private static Regex LinkScrape = new Regex("<a\\s+(?:[^>]*?\\s+)?href=\"([^ \"]*)\"");
    private static Regex LinkStart = new Regex("^(\\/\\/|https?:\\/\\/)");

    private static string Title {
      get {
        if (string.IsNullOrEmpty(_title)) { _title = ConfigurationManager.AppSettings["WebCrawler:Name"]; }
        return _title;
      }
    }
    private static string Version {
      get {
        if (string.IsNullOrEmpty(_version)) { _version = ConfigurationManager.AppSettings["WebCrawler:Version"]; }
        return _version;
      }
    }
    private static int Queue {
      get {
        if (_queue == 0) { _queue = Convert.ToInt32(ConfigurationManager.AppSettings["WebCrawler:DomainPageQueue"]); }
        return _queue;
      }
    }

    public static HttpClient Crawler {
      get {
        if (_client == null) {
          _client = new HttpClient();
          _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Title, Version));
        }

        return _client;
      }
    }

    private Domain Domain { get; set; }
    private RobotUserAgent UserAgent { get; set; }
    private List<string> Documents { get; set; }

    public WebCrawler(string host) {
      Domain = context.Domains.FirstOrDefault(x => x.Name == host);
      Documents = context.Documents.Where(x => x.Host == host).Select(x => x.Url).ToList();
      SetUserAgent();
    }

    private void SetUserAgent() {
      UserAgent = null;

      if (Domain.RobotTexts == null || Domain.RobotTexts.LastUpdated <= DateTime.UtcNow.AddDays(1)) {
        Domain.RobotTexts = RobotsText.ProcessRobotFile(string.Format("http://{0}", Domain.Name));
        context.Domains.Update(Domain.ID.ToString(), Domain);
      }

      UserAgent = Domain.RobotTexts.UserAgents.FirstOrDefault(x => x.Name == Title);
      if (UserAgent == null) {
        UserAgent = Domain.RobotTexts.UserAgents.FirstOrDefault(x => x.Name == "*");
      }
    }

    public async Task<bool> CrawlPages() {
      var docs = context.Documents.Where(x => (x.Indexed == false || x.LastIndexed <= DateTime.UtcNow.AddDays(-2)) && x.Host == Domain.Name).Take(Queue);

      foreach (var doc in docs) {
        var uri = new Uri(doc.Url);
        var root = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));

        MatchCollection matches = null;

        try {
          var page = await Crawler.GetAsync(doc.Url);
          doc.Indexed = true;
          doc.StatusCode = (int)page.StatusCode;

          if (page.IsSuccessStatusCode) {
            var mime = page.Content.Headers.ContentType.MediaType;

            if (mime == "text/html") {
              var content = await page.Content.ReadAsStringAsync();
              matches = LinkScrape.Matches(content);
              
              if (matches != null) {
                foreach (Match m in matches) {
                  var href = m.Groups[1].Value.Trim();

                  if (href[0] == '#') { continue; }
                  if ((href.ToLower().StartsWith("://") || href.ToLower().StartsWith("https://")  || href.ToLower().StartsWith("http://")) && !href.Contains(Domain.Name)) { continue; }

                  var link = new Uri(root, href);
                  if (!UserAgent.CanIndex(link)) { continue; }
                  if (!Documents.Contains(link.ToString()) && link.ToString().Contains(Domain.Name)) {
                    doc.Links.Add(link.ToString());
                    Documents.Add(link.ToString());
                    context.Documents.Add(new Document { Host = Domain.Name, Url = link.ToString(), Indexed = false, LastIndexed = DateTime.Now });
                  }
                }
              }
            }

            doc.MimeType = mime;
            doc.Indexed = true;
            doc.LastIndexed = DateTime.Now;
            doc.StatusCode = (int)page.StatusCode;
            context.Documents.Update(doc.ID.ToString(), doc);

            var count = context.Documents.Count(x => x.Host == Domain.Name);
            Domain.Documents = count;
            context.Domains.Update(Domain.ID.ToString(), Domain);

          }
          else {
            doc.Indexed = true;
            doc.LastIndexed = DateTime.Now;
            doc.StatusCode = (int)page.StatusCode;
            context.Documents.Update(doc.ID.ToString(), doc);
          }
        }
        catch {

        }
      }
      return true;
    }
  }
}
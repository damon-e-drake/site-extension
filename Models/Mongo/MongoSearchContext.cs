using Mongo.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Excavator.Models.Mongo {
  public class MongoSearchContext : MongoDBContext {
    public MongoCollectionSet<Document> Documents { get; set; }
    public MongoCollectionSet<Domain> Domains { get; set; }
    public MongoSearchContext() : base("name=MongoSearch") {

    }
  }

  [BsonIgnoreExtraElements, CollectionName("Domains")]
  public class Domain {
    [BsonId]
    public BsonObjectId ID { get; set; }
    [BsonIgnoreIfNull, BsonElement("name")]
    public string Name { get; set; }
    [BsonElement("documents")]
    public int Documents { get; set; }
  }

  public class RobotsText {
    private static string[] Rules = new[] { "crawl-delay", "allow", "disallow", "noindex", "sitemap" };

    [BsonElement("file")]
    public string File { get; private set; }
    [BsonElement("file")]
    public DateTime LastUpdated { get; set; }
    [BsonIgnoreIfNull, BsonElement("userAgents")]
    public IList<RobotUserAgent> UserAgents { get; set; }
    [BsonIgnoreIfNull, BsonElement("sitemaps")]
    public IList<string> SiteMaps { get; set; }

    public static RobotsText ProcessRobotFile(string url) {
      var contents = string.Empty;

      using (var client = new HttpClient()) {
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Excavationbot", "1.0"));
        try {
          var text = client.GetAsync(BaseUri).Result;

          if (text.IsSuccessStatusCode) { contents = text.Content.ReadAsStringAsync().Result; }
        }
        catch {

        }
      }

      var table = ParseBotFile(contents);
      if (table == null) { return; }

      var userAgents = new List<RobotUserAgent>();
      var agents = table.Where(x => x.Name != null).Select(x => x.Name).Distinct();
      foreach (var agent in agents) {
        var a = new RobotUserAgent() {
          Name = agent,
          Allows = table.Where(x => x.Name == agent && x.Rule == "allow").Select(f => f.Path),
          Disallows = table.Where(x => x.Name == agent && x.Rule == "disallow").Select(f => f.Path),
          NoIndexes = table.Where(x => x.Name == agent && x.Rule == "noindex").Select(f => f.Path)
        };

        var delay = table.FirstOrDefault(x => x.Name == agent && x.Rule == "crawl-delay");
        if (delay != null) { a.CrawlDelay = int.Parse(delay.Path); }

        userAgents.Add(a);
      }

      UserAgents = userAgents;
      SiteMaps = table.Where(x => x.Rule == "sitemap").Select(x => x.Path).ToList();
    }

    public bool CanIndex(Uri uri) {
      var agent = UserAgents.FirstOrDefault(x => x.Name == "*");
      if (agent == null) { return false; }

      var path = uri.AbsolutePath.ToString();
      if (agent.Disallows.Any(x => path.StartsWith(x)) || agent.NoIndexes.Any(x => path.StartsWith(x))) { return false; }

      return true;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();

      foreach (var agent in UserAgents) {
        sb.AppendFormat("user-agent: {0}\n", agent.Name);
        if (agent.CrawlDelay > 0) { sb.AppendFormat("crawl-delay: {0}\n", agent.CrawlDelay); }
        foreach (var s in agent.Allows.OrderBy(x => x)) { sb.AppendFormat("allow: {0}\n", s); }
        foreach (var s in agent.Disallows.OrderBy(x => x)) { sb.AppendFormat("disallow: {0}\n", s); }
        foreach (var s in agent.NoIndexes.OrderBy(x => x)) { sb.AppendFormat("noindex: {0}\n", s); }
        sb.AppendLine("");
      }

      foreach (var s in SiteMaps) { sb.AppendFormat("sitemap: {0}\n", s); }

      return sb.ToString();
    }

    private static IEnumerable<UserAgentTable> ParseBotFile(string contents) {
      if (string.IsNullOrEmpty(contents)) { return null; }
      var lines = contents.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

      var table = new List<UserAgentTable>();
      var curAgent = string.Empty;

      for (int i = 0; i < lines.Length; i++) {
        var line = lines[i].Trim().Split(new[] { ':' }, 2);

        if (line.Length != 2) { continue; }

        if (Rules.Contains(line[0].Trim().ToLower())) {
          table.Add(new UserAgentTable { Name = curAgent, Rule = line[0].Trim().ToLower(), Path = line[1].Trim() });
          continue;
        }
        if (line[0].Equals("user-agent", StringComparison.InvariantCultureIgnoreCase)) {
          curAgent = line[1].Trim();
          continue;
        }
      }
      return table;
    }
  }

  public class RobotUserAgent {
    public string Name { get; set; }
    public int CrawlDelay { get; set; }
    public IEnumerable<string> Allows { get; set; }
    public IEnumerable<string> Disallows { get; set; }
    public IEnumerable<string> NoIndexes { get; set; }

    public RobotUserAgent() {
      Allows = new List<string>();
      Disallows = new List<string>();
      NoIndexes = new List<string>();
    }
  }

  public class UserAgentTable {
    public string Name { get; set; }
    public string Rule { get; set; }
    public string Path { get; set; }
  }

  [BsonIgnoreExtraElements, CollectionName("Documents")]
  public class Document {
    [BsonId]
    public BsonObjectId ID { get; set; }
    [BsonIgnoreIfNull, BsonElement("domain")]
    public string Domain { get; set; }
    [BsonElement("url")]
    public string Url { get; set; }
  }
}
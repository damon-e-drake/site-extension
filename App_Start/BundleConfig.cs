using System.Web;
using System.Web.Optimization;

namespace Excavator {
  public class BundleConfig {
    public static void RegisterBundles(BundleCollection bundles) {
      bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js", "~/Scripts/knockout-{version}.js"));
      bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));
      bundles.Add(new ScriptBundle("~/bundles/ko-models").Include("~/Scripts/ko-models/*.js"));
      // Add bundle for Knockout Models

      bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/bootstrap.css","~/Content/font-awesome.css", "~/Content/site.css"));
    }
  }
}

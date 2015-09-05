using System.Web;
using System.Web.Optimization;

namespace Ricettario
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //    "~/Content/bootstrap.css",
            //    "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/AngularJs")
                .Include(
                    "~/Scripts/jquery-1.10.2.js",
                    "~/Scripts/bootstrap.js",
                    //"~/Scripts/angular.js",
                    //"~/Scripts/angular-route.js",
                    //"~/Scripts/angular-ui/ui-bootstrap-tpls.js",
                    //"~/Scripts/angular-ui/ui-grid-unstable.js",
                    "~/Scripts/linq.js",
                    "~/Scripts/knockout-3.2.0.js",
                    //"~/Scripts/prettify.js",
                    //"~/Scripts/knockout2.js",
                    //"~/Scripts/knockout-bootstrap.js",
                    "~/Scripts/knockout.mapping-latest.js",
                    "~/Scripts/typeahead.bundle.js"
                //"~/Content/Selectize/js/standalone/selectize.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/AppMain")
                .IncludeDirectory("~/js/Controllers", "*.js")
                .Include("~/js/AppMain.js"));

            bundles.Add(new ScriptBundle("~/bundles/Model")
                .Include("~/js/dictionary.js")
                .Include("~/js/RestBackedViewModel.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/bootstrap.css",
                "~/Content/ui-grid-unstable.css",
                //"~/Content/Selectize/css/selectize.css",
                //"~/Content/Selectize/css/selectize.bootstrap3.css",
                //"~/Content/Selectize/css/selectize.default.css",
                //"~/Content/typeaheadjs.css",
                "~/Content/typeaheadjs-example.css",
                "~/Content/site.css"));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            //BundleTable.EnableOptimizations = true;

        }
    }
}

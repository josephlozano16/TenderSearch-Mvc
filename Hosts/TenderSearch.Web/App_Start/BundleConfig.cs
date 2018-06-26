using System.Web.Optimization;

namespace TenderSearch.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            #region JAVASCRIPTS
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/client").Include(

            "~/Scripts/globalize.0.1.3/globalize.js",
            "~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*",
                "~/Scripts/bootstrap.js",
                "~/Scripts/client.js"));
            #endregion // JAVASCRIPTS

            #region CSS
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/ie10mobile.css", // Must be first. IE10 mobile viewport fix
                "~/Content/font-awesome.css",
                "~/Content/bootstrap.css",
                "~/Content/themes/base/jquery-ui.css",
                "~/Content/themes/Cupertino/*.css",
                "~/Content/Site.css",
                "~/Content/client.css",
                "~/Content/eml-checkbox.css",
                "~/Content/animate.css "
            ));
            #endregion // CSS


            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));

            //bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            //            "~/Scripts/jquery.validate*"));

            //// Use the development version of Modernizr to develop with and learn from. Then, when you're
            //// ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));

            var enableOptimizations = true;
#if DEBUG
            enableOptimizations = false;
#endif
            //minify all bundles of css and javascripts
            BundleTable.EnableOptimizations = enableOptimizations;
        }
    }
}

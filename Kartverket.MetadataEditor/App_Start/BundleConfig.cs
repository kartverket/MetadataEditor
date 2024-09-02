using System.Web;
using System.Web.Optimization;

namespace Kartverket.MetadataEditor
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/bower_components/kartverket-felleskomponenter/assets/css/styles").Include(
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendor.min.css",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/vendorfonts.min.css",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/css/main.min.css",
               "~/Content/temp.css"
               ));

            bundles.Add(new ScriptBundle("~/bundle/js").Include(
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/vendor.min.js",
               //"~/Content/bower_components/vue/dist/vue.min.js",
               //"~/Content/bower_components/vuex/dist/vuex.min.js",
               "~/Content/bower_components/kartverket-felleskomponenter/assets/js/main.min.js"
               ));

            bundles.Add(new ScriptBundle("~/Scripts/local-scripts").Include(
               "~/Scripts/jquery.validate.js",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/expressive.annotations.validate.js",
                "~/Scripts/globalize.js",
                "~/Scripts/globalize.culture.nb-NO.js",
                "~/Scripts/globalize-custom.js",
                "~/Scripts/respond.js",
                "~/Scripts/jquery-ui-{version}.js",
                "~/Scripts/jQuery.FileUpload/jquery.iframe-transport.js",
                "~/Scripts/jQuery.FileUpload/jquery.fileupload.js",
                "~/Scripts/bootstrap-filestyle.js",
                "~/Scripts/geonorge-editor.js",
                "~/Scripts/jquery.autosize.js"
           ));

            bundles.Add(new ScriptBundle("~/node-modules/scripts").Include(
               "~/node_modules/@kartverket/geonorge-web-components/MainNavigation.js",
               "~/node_modules/@kartverket/geonorge-web-components/GeoNorgeFooter.js"
            ));

            BundleTable.EnableOptimizations = true;
        }
    }
}

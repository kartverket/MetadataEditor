using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Kartverket.MetadataEditor.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlString ApplicationVersionNumber(this HtmlHelper helper)
        {
            string versionNumber = WebConfigurationManager.AppSettings["BuildVersionNumber"];
            return MvcHtmlString.Create(versionNumber);
        }
        public static string GeonorgeUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["GeonorgeUrl"];
        }
        public static string GeonorgeArtiklerUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["GeonorgeArtiklerUrl"];
        }
        public static string NorgeskartUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["NorgeskartUrl"];
        }
        public static string RegistryUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["RegistryUrl"];
        }
        public static string ObjektkatalogUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["ObjektkatalogUrl"];
        }
        public static string KartkatalogUrl(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["KartkatalogUrl"];
        }

        public static bool SimpleMetadataEnabled(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["SimpleMetadataEnabled"] == "false" ? false : true;
        }

        public static string EnvironmentName(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["EnvironmentName"];
        }
        public static string WebmasterEmail(this HtmlHelper helper)
        {
            return WebConfigurationManager.AppSettings["WebmasterEmail"];
        }
    }
}
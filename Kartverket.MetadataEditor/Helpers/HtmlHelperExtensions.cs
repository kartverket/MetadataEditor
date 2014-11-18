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
    }
}
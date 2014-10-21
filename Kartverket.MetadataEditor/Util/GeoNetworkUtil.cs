using System.Web.Configuration;

namespace Kartverket.MetadataEditor.Util
{
    public static class GeoNetworkUtil
    {
        public static string GetViewUrl(string uuid)
        {
            return string.Format("{0}?uuid={1}", GetBaseUrl(), uuid);
        }

        private static string GetBaseUrl()
        {
            return WebConfigurationManager.AppSettings["GeoNetworkUrl"];
        }

        public static string GetXmlDownloadUrl(string uuid)
        {
            return string.Format("{0}srv/nor/xml_iso19139?uuid={1}", GetBaseUrl(), uuid);
        }
    }
}
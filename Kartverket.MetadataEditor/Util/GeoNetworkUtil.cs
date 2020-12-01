using System.Collections.Generic;
using System.Security.Claims;
using System.Web.Configuration;
using Geonorge.AuthLib.Common;

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
        
        public static Dictionary<string, string> CreateAdditionalHeadersWithUsername(string username, string published = "")
        {
            var headers = new Dictionary<string, string> { { "GeonorgeUsername", username } };

            ClaimsPrincipal currentUser = ClaimsPrincipal.Current;
            headers.Add("GeonorgeOrganization", currentUser.GetOrganizationName());

            var isAdmin = false;
            var editorRole = false;

            if (currentUser.IsInRole(GeonorgeRoles.MetadataAdmin))
                isAdmin = true;
            
            if (currentUser.IsInRole(GeonorgeRoles.MetadataEditor))
                editorRole = true;

            if (isAdmin)
                headers.Add("GeonorgeRole", GeonorgeRoles.MetadataAdmin);
            else if (editorRole)
                headers.Add("GeonorgeRole", GeonorgeRoles.MetadataEditor);

            headers.Add("published", published);

            return headers;
        }

        public static string GetCoordinatesystemText(string coordinateSystem)
        {
            if (string.IsNullOrEmpty(coordinateSystem))
                return null;

            string coordinateSystemtext = coordinateSystem;
            string coordinateSystemCode = coordinateSystem.Substring(coordinateSystem.LastIndexOf('/') + 1);
            if (!string.IsNullOrEmpty(coordinateSystemCode))
            {
                if (!coordinateSystemCode.StartsWith("EPSG"))
                    coordinateSystemtext = "EPSG:" + coordinateSystemCode;
                else
                    coordinateSystemtext = coordinateSystemCode;
            }

            return coordinateSystemtext;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using GeoNorgeAPI;
using www.opengis.net;
using System.Threading.Tasks;
using Kartverket.Geonorge.Utilities.Organization;
using System.Web.Configuration;

namespace Kartverket.MetadataEditor.Models.ProductSheet
{
    public class ProductSheetService
    {

        public string GetLogoForOrganization(string organization)
        {
            if (organization != null)
            {
                OrganizationService organizationService = new OrganizationService(WebConfigurationManager.AppSettings["RegistryUrl"], new HttpClientFactory());
                Task<Organization> getOrganizationTask = organizationService.GetOrganizationByName(organization);
                Organization organizationInfo = getOrganizationTask.Result;
                if (organizationInfo != null)
                {
                    return organizationInfo.LogoLargeUrl;
                }
            }

            return null;
        }


        public string GetReferenceSystemName(string coordinateSystem)
        {
            System.Net.WebClient c = new System.Net.WebClient();
            c.Encoding = System.Text.Encoding.UTF8;
            var data = c.DownloadString(System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/register/epsg-koder");
            var response = Newtonsoft.Json.Linq.JObject.Parse(data);

            var refs = response["containeditems"];

            foreach (var refsys in refs)
            {

                var documentreference = refsys["documentreference"].ToString();
                if (documentreference == coordinateSystem)
                {
                    coordinateSystem = refsys["label"].ToString();
                    break;
                }
            }

            return coordinateSystem;
        }

    }
}
using Kartverket.Geonorge.Utilities.Organization;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Kartverket.MetadataEditor.Models.Rdf
{
    public class AdministrativeUnitService : IAdministrativeUnitService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IHttpClientFactory _httpClientFactory;

        string endPointUri = "http://rdf.kartverket.no/api/1.0/adminstrative_unit/search?search=";

        public AdministrativeUnitService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private async Task<AdministrativeUnit> FetchAdministrativeUnitAsync(string search)
        {
            string url = endPointUri + search;
            Log.Info("Fetching adminstrative_unit uri: " + url);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage response = await _httpClientFactory.GetHttpClient().SendAsync(request).ConfigureAwait(false);
            Log.Debug($"Status from [adminstrative_unit={url}] was {response.StatusCode}");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<AdministrativeUnit>().ConfigureAwait(false);
        }

        public List<string> UpdateKeywordsPlaceWithUri(List<string> keywordsPlace)
        {
            if (keywordsPlace != null)
            {
                for (int k = 0; k < keywordsPlace.Count(); k++)
                {
                    string uri = GetAdministativeUnitUri(keywordsPlace[k]);
                    if (!string.IsNullOrEmpty(uri))
                        keywordsPlace[k] = keywordsPlace[k] + "|" + uri;
                }
            }

            return keywordsPlace;
        }

        private string GetAdministativeUnitUri(string area)
        {
            string url = "";
            try
            {
                var adminUnit = FetchAdministrativeUnitAsync(area);
                if (adminUnit != null)
                {
                    url = adminUnit.Result.results.bindings[0].uri.value;
                }
            }
            catch (Exception) { }

            return url;
        }

    }
}
using Kartverket.Geonorge.Utilities.Organization;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using IHttpClientFactory = Kartverket.Geonorge.Utilities.Organization.IHttpClientFactory;

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

        public void UpdateKeywordsAdministrativeUnitsWithUri(ref List<string> keywordsAdministrativeUnits, ref List<string> keywordsPlace)
        {
            if (keywordsPlace != null)
            {
                int numberOfPlaces = keywordsPlace.Count();
                for (int k = 0; k < numberOfPlaces; k++)
                {
                    string uri = GetAdministativeUnitUri(keywordsPlace[k]);
                    if (!string.IsNullOrEmpty(uri)) {
                        keywordsAdministrativeUnits.Remove(keywordsPlace[k]);
                        keywordsAdministrativeUnits.Add(keywordsPlace[k] + "|" + uri);
                        keywordsPlace.Remove(keywordsPlace[k]);
                        k--; numberOfPlaces--;
                    }
                }
            }
        }

        private string GetAdministativeUnitUri(string area)
        {
            string url = "";
            try
            {
                var adminUnit = FetchAdministrativeUnitAsync(area);
                if (adminUnit != null && adminUnit.Result.results.bindings.Length == 1)
                {
                    url = adminUnit.Result.results.bindings[0].uri.value;
                }
            }
            catch (Exception) { }

            return url;
        }

    }
}
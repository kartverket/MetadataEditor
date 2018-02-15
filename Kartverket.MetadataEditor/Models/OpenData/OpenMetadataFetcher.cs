using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Kartverket.Geonorge.Utilities.Organization;
using log4net;

namespace Kartverket.MetadataEditor.Models.OpenData
{
    internal class OpenMetadataFetcher : IOpenMetadataFetcher
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IHttpClientFactory _httpClientFactory;

        public OpenMetadataFetcher(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Metadata> FetchMetadataAsync(OpenMetadataEndpoint endpoint)
        {
            Log.Info("Fetching open metadata from: " + endpoint);
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Url);
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage response = await _httpClientFactory.GetHttpClient().SendAsync(request).ConfigureAwait(false);
            Log.Debug($"Status from [endpoint={endpoint.Url}] was {response.StatusCode}");
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<Metadata>().ConfigureAwait(false);
        }
    }
}
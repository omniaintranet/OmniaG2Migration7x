using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class SharePointImageHttpClient : BaseHttpClientService, IHttpImageClient
    {
        private string Cookie
        {
            get
            {
                return MigrationSettings.Value.SharePointSecuritySettings.AuthCookie;
            }
        }

        protected override string BaseUrl => string.Empty;

        public SharePointImageHttpClient(
          IHttpClientFactory httpClientFactory,
          IOptionsSnapshot<MigrationSettings> migrationSettings)
          : base(httpClientFactory, migrationSettings)
        {
        }

        protected override void EnsureDefaultHeaders(HttpRequestHeaders headers)
        {
            headers.Accept.Clear();
            headers.Add("Cookie", Cookie);
        }

        public async ValueTask<byte[]> GetImage(string imageUrl)
        {
            var parameters = new NameValueCollection();

            var httpResponse = await GetAsync(imageUrl, parameters: parameters);
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Image not found in G1 " + imageUrl + ". Response status: " + httpResponse.StatusCode.ToString());
            }
            else if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK && httpResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Cannot get image at URL " + imageUrl + ". Response status: " + httpResponse.StatusCode.ToString());
            }

            var apiResponse = httpResponse.Content.ReadAsByteArrayAsync();

            return await apiResponse;
        }
    }
}

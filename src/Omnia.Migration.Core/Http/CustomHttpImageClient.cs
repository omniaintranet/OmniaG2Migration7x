using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class CustomHttpImageClient : BaseHttpClientService, IHttpImageClient
    {
        private CustomHttpImageClientSettings ClientSettings
        {
            get
            {
                return MigrationSettings.Value.CustomImageClient;
            }
        }

        protected override string BaseUrl => string.Empty;

        public CustomHttpImageClient(
          IHttpClientFactory httpClientFactory,
          IOptionsSnapshot<MigrationSettings> migrationSettings)
          : base(httpClientFactory, migrationSettings)
        {
        }

        protected override void EnsureDefaultHeaders(HttpRequestHeaders headers)
        {
            headers.Accept.Clear();
            switch (this.ClientSettings.AuthorizeMethod)
            {
                case AuthorizationMethod.Authorization:
                    headers.Add("Authorization", ClientSettings.Authorization);
                    break;
                case AuthorizationMethod.Token:
                    headers.Add("Token", ClientSettings.Token);
                    break;
                case AuthorizationMethod.Cookie:
                    headers.Add("Cookie", ClientSettings.Cookie);
                    break;
                default:
                    break;
            }
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

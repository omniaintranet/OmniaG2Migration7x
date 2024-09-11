using Microsoft.Extensions.Options;
using Omnia.Foundation.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.BlockData;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class G1SearchPropertiesHttpClient: G1HttpClientService
    {
        protected override string BaseUrl
        {
            get
            {
                return $"{MigrationSettings.Value.OmniaG1Settings.IntranetUrl}/api/searchproperty";
            }
        }

        public G1SearchPropertiesHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
        }

        public async ValueTask<ApiOperationResult<List<G1SearchProperty>>> GetSearchPropertiesAsync(int category)
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", MigrationSettings.Value.WCMContextSettings.SharePointUrl },
                    { "category", category.ToString() }
                };

            var httpResponse = await GetAsync("/getsearchproperties", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<List<G1SearchProperty>>>();

            return await apiResponse;
        }
    }
}

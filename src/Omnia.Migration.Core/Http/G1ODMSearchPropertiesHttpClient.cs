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
    public class G1ODMSearchPropertiesHttpClient : G1HttpClientService
    {
        protected override string BaseUrl
        {
            get
            {
                return $"{MigrationSettings.Value.OmniaG1Settings.ODMUrl}/SearchProperty";
            }
        }

        public G1ODMSearchPropertiesHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
        }

        public async ValueTask<ApiOperationResult<List<G1SearchProperty>>> GetSearchPropertiesAsync()
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", MigrationSettings.Value.WCMContextSettings.SharePointUrl },
                    { "Lang", "sv-se" },
                    { "excludeSystemColumns", "false" },
                    { "requireRelatedDocumentPropety", "false" },
                };

            var httpResponse = await GetAsync("/GetSearchProperties", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<List<G1SearchProperty>>>();

            return await apiResponse;
        }
    }
}

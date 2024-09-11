using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.EnterpriseProperties;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class EnterprisePropertiesApiHttpClient : G2HttpClientService
    {// Omnia.Fx.Identities.HttpContract.IdentityService.Routes.Query
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("bb000000-0000-bbbb-0000-0000000000bb"));
            }
        }

        public EnterprisePropertiesApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<List<EnterprisePropertyDefinition>>> GetEnterprisePropertiesAsync()
        {
            var parameters = new NameValueCollection()
                {
                    
                };

            var httpResponse = await GetAsync("api/enterpriseproperties", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<List<EnterprisePropertyDefinition>>>();

            return await apiResponse;
        }
    }
}

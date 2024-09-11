using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.Apps;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Links;
using Omnia.WebContentManagement.Fx.Services;

namespace Omnia.Migration.Core.Http
{
    public class AppTemplatesApiHttpClient: G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {                
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("bb000000-0000-bbbb-0000-0000000000bb"));
            }
        }

        public AppTemplatesApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }


        public async ValueTask<ApiResponse<List<AppTemplate>>> GetAppTemplates(Guid profileId)
        {
            var parameters = new NameValueCollection()
                {
                    { "profileId", profileId.ToString()}
                };

            var httpResponse = await GetAsync("api/apps/profiles/templates", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<List<AppTemplate>>>();

            return await apiResponse;
        }
    }
}

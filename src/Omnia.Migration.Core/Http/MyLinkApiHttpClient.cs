using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Links;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class MyLinkApiHttpClient : G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                {
                    // Omnia.Workplace.Web serviceId
                    return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("39df27aa-95f1-4a23-b3f6-8b231afcda82"));
                }
            }
        }

        public MyLinkApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<QuickLink>> AddOrUpdateMyLinkAsync(QuickLink link)
        {
            var parameters = new NameValueCollection()
                {
                    { Fx.Constants.Parameters.IsSystemUpdate, "true" }
                };

            var httpResponse = await PostAsJsonAsync("api/mylinks", link, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<QuickLink>>();
            Thread.Sleep(500);
            return await apiResponse;
        }
    }
}

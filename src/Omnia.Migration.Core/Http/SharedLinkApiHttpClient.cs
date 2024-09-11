using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Links;
using Omnia.WebContentManagement.Fx.Services;

namespace Omnia.Migration.Core.Http
{
    public class SharedLinkApiHttpClient: G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                // Omnia.Workplace.Web serviceId
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("39df27aa-95f1-4a23-b3f6-8b231afcda82"));
            }
        }

        public SharedLinkApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }


        public async ValueTask<ApiResponse<QuickLink>> AddOrUpdateSharedLink(QuickLink link)
        {
            string apiLink = "api/sharedlinks/tenant";
            var parameters = new NameValueCollection()
                {
                    //{ Fx.Constants.Parameters.IsSystemUpdate, "true" },
                    {"profileId",MigrationSettings.Value.ImportLinksSettings.BusinessProfileId }
                };
            
            //09082022 - Diem: add link in the given business profile, instead of tenant scope.
            if(MigrationSettings.Value.ImportLinksSettings.BusinessProfileId.IsNotNull())
            {
               apiLink = "api/sharedlinks";               
            }
            var httpResponse = await PostAsJsonAsync(apiLink, link, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<QuickLink>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<List<QuickLink>>> GetAllSharedLinks()
        {
            var httpResponse = await GetAsync("api/sharedlinks/all");
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<List<QuickLink>>>();

            return await apiResponse;
        }
    }
}

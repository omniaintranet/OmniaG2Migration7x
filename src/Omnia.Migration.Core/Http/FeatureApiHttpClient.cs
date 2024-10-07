using Microsoft.Extensions.Options;
using Omnia.Fx.Models.Apps;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.BusinessProfiles;
using Omnia.Fx.Models.EnterpriseProperties;
using Omnia.Fx.Models.Features;
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
    public class FeatureApiHttpClient : G2HttpClientService
    {// Omnia.Fx.Identities.HttpContract.IdentityService.Routes.Query
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("bb000000-0000-bbbb-0000-0000000000bb"));
            }
        }

        public FeatureApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<FeatureInstance>> FeatureActivateAsync(string featureid,string appinstantid)
        {
            var parameters = new NameValueCollection()
                {
                    { "appinstanceid", appinstantid }
                };

            var httpResponse = await PostAsJsonAsync("api/features/"+ featureid, (object)null, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<FeatureInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<FeatureInstance>> FeatureReActivateAsync(string featureid, string appinstantid)
        {
            var parameters = new NameValueCollection()
                {
                    { "appinstanceid", appinstantid }
                };

            var httpResponse = await PutAsJsonAsync("api/features/" + featureid, (object)null, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<FeatureInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<FeatureInstance>> FeatureDeActivateAsync(string featureid, string appinstantid)
        {
            var parameters = new NameValueCollection()
                {
                    { "appinstanceid", appinstantid }
                };

            var httpResponse = await DeleteAsync("api/features/" + featureid,  parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<FeatureInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<AppInstancesResult>> GetAppInstancesAsync(Guid appDefinitionId, Guid profileId, bool showInPublicListingsOnly = true, bool showStatusErrorsOnly = false)
        {
            var parameters = new NameValueCollection()
            {
                {"profileId", profileId.ToString() },
                {"showInPublicListingsOnly", showInPublicListingsOnly ? "true": "false"  }
            };

            var appStatuses = new List<AppInstanceStatus> { AppInstanceStatus.Ready, AppInstanceStatus.Error };

            if (showStatusErrorsOnly)
            {
                appStatuses.Remove(AppInstanceStatus.Ready);
            }

            var query = new AppInstanceQuery
            {
                IncludeTotal = false,
                ItemLimit = 20099,
                Skip = 0,
                Statuses = appStatuses,
                KeywordFilterEnableWildcardSearch = true,
                OrderBy = new OrderBy
                {
                    PropertyName = "Title",
                    Descending = false
                }
            };

            var httpResponse = await PostAsJsonAsync($"/api/apps/profiles/definitions/{appDefinitionId}/instances", query, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstancesResult>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<List<BusinessProfile>> >GetBPAsync()
        {
            var httpResponse= await GetAsync($"/api/businessprofiles");
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<List<BusinessProfile>>>();
            return await apiResponse;


        }
      
    }
}

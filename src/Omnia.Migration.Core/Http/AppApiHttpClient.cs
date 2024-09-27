using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Omnia.Foundation.Models.Features;
using Omnia.Fx.Models.Apps;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.BusinessProfiles;
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
    public class AppApiHttpClient : G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("bb000000-0000-bbbb-0000-0000000000bb"));
            }
        }

        public AppApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<IList<BusinessProfile>>> GetBusinessProfilesAsync()
        {
            var parameters = new NameValueCollection()
            {
            };

            var httpResponse = await GetAsync("/api/businessprofiles", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<BusinessProfile>>>();

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
        public async ValueTask<ApiResponse<FeatureInstance>> FeatureActivateAsync(string featureid, string appinstantid)
        {
            var parameters = new NameValueCollection()
                {
                    { "appinstanceid", appinstantid }
                };

            var httpResponse = await PostAsJsonAsync("api/features/" + featureid, (object)null, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<FeatureInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<FeatureInstance>> FeatureDeActivateAsync(string featureid, string appinstantid)
        {
            var parameters = new NameValueCollection()
                {
                    { "appinstanceid", appinstantid }
                };

            var httpResponse = await DeleteAsync("api/features/" + featureid, parameters: parameters);
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

            var appStatuses = new List<AppInstanceStatus> { AppInstanceStatus.Ready, AppInstanceStatus.Error, AppInstanceStatus.ReadyWithWarning };

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

        public async ValueTask<ApiResponse<AppInstance>> CreateAppInstanceAsync(Guid profileId, Guid appTemplateId, string siteUrl, AppInstanceInputInfo inputInfo)
        {
            var parameters = new NameValueCollection()
                {
                    { "profileId", profileId.ToString() },
                    { "appTemplateId", appTemplateId.ToString() },
                    { "spUrl", siteUrl }
                };

            var httpResponse = await PostAsJsonAsync("/api/apps/instance", inputInfo, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<AppInstance>> UpdateAppInstanceEnterprisePropertiesAsync(Guid profileId, Guid appInstanceId, string spUrl, Dictionary<string, object> appInstanceProperties)
        {
            var parameters = new NameValueCollection()
                {
                    { "appInstanceId", appInstanceId.ToString() },
                    { "profileId", profileId.ToString() },
                    { "spurl",spUrl }
                };

            var httpResponse = await PutAsJsonAsync("/api/apps/instance", appInstanceProperties, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<AppInstance>> UpdateAppInstanceEnterprisePropertiesAsync1(Guid profileId, Guid appInstanceId, string spUrl, AppInstanceInputInfo appInstanceProperties)
        {
            var parameters = new NameValueCollection()
                {
                    { "appInstanceId", appInstanceId.ToString() },
                    { "profileId", profileId.ToString() },
                    { "spurl",spUrl }
                };

            var httpResponse = await PutAsJsonAsync("/api/apps/instance", appInstanceProperties, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }



        public async ValueTask<ApiResponse<AppInstance>> UpdateAppInstancePermissionAsync(Guid profileId, Guid appInstanceId, JObject payLoad)
        {
            var parameters = new NameValueCollection()
                {{ "omniaapp","false" },
                { "profileId", profileId.ToString() },
                    { "appinstanceid", appInstanceId.ToString() }

                };

            var httpResponse = await PostAsJsonAsync("/api/security/update/settings", payLoad, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<AppInstance>> GetAppInstanceStatusAsync(Guid profileId, Guid appInstanceId)
        {
            var parameters = new NameValueCollection()
                {
                    { "profileId", profileId.ToString() },
                    { "appInstanceId", appInstanceId.ToString() }
                };

            var httpResponse = await GetAsync("/api/apps/instance/polling", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<AppInstance>> DeleteAppInstanceAsync(Guid profileId, Guid appInstanceId)
        {
            var parameters = new NameValueCollection()
                {
                    { "appInstanceId", appInstanceId.ToString() },
                    { "profileId", profileId.ToString() }
                };
            var headers = new NameValueCollection()
                {
                    { "Cookie", MigrationSettings.Value.MigrateCustomLink.Cookie },
                    { "Accept",MigrationSettings.Value.MigrateCustomLink.Accept }
                };
            MigrationSettings.Value.MigrateCustomLink.MigrateCustomLinktoG2 = true;

            var httpResponse = await DeleteAsync("/api/apps/instance", parameters: parameters, headers: headers);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<AppInstance>>();

            return await apiResponse;
        }
    }
}

using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.WebContentManagement.Fx.Services;
using Omnia.WebContentManagement.Models.ChannelManagement;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using PublishingChannelCategory = Omnia.WebContentManagement.Models.ChannelManagement.PublishingChannelCategory;

namespace Omnia.Migration.Core.Http
{
    public class PublishingChannelApiHttpClient : G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl => OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);

        public PublishingChannelApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<List<PublishingChannelCategory>> GetAllChannelCategoriesAsync()
        {
            var httpResponse = await GetAsync("api/publishingchannelcategories");
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<List<PublishingChannelCategory>>>();

            return apiResponse.Data;
        }

        public async ValueTask<ApiResponse<PublishingChannelCategory>> CreateChannelCategoryAsync(PublishingChannelCategory category)
        {
            var httpResponse = await PostAsJsonAsync("/api/publishingchannelcategories", category);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<PublishingChannelCategory>>();

            return apiResponse;
        }

        public async ValueTask<ApiResponse<PublishingChannel>> CreatePublishingChannelAsync(ChannelCreateRequest request)
        {
            var httpResponse = await PostAsJsonAsync(HttpContract.ChannelManagement.Routes.BaseRoute, request);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<PublishingChannel>>();

            return apiResponse;
        }

        public async ValueTask<ApiResponse> PublishPageToChannelsAsync(int pageId, IList<int> publishingChannelIds, int defaultPublishingChannelId)
        {
            var parameters = new NameValueCollection()
                {
                    { "pageId", pageId.ToString() },
                    { "defaultPublishingChannelId", defaultPublishingChannelId.ToString() }
                };
            
            List<PublishChannelPageRequest> publishingChannels = new List<PublishChannelPageRequest>();
            foreach(var publishingChannelId in publishingChannelIds)
            {
                publishingChannels.Add(new PublishChannelPageRequest() { ChannelId = publishingChannelId, PendingApproval = false });
            }

            var httpResponse = await PostAsJsonAsync(HttpContract.ChannelManagement.Routes.PublishPageToChannels, publishingChannels, parameters: parameters);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse>();

            return apiResponse;
        }
    }
}

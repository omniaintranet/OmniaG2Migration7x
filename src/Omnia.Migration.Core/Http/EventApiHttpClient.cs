using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class EventApiHttpClient : G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public EventApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings) : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<EventDetail>> EnsureEventAsync(EventDetail eventDetail)
        {
            eventDetail.OutlookEventId = null;
            var httpResponse = await PostAsJsonAsync("/api/events/ensureevent", eventDetail);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<EventDetail>>();

            return apiResponse;
        }

        public async ValueTask<ApiResponse> CloneEventAsync(EventClone eventClone)
        {
            var httpResponse = await PostAsJsonAsync("/api/events/cloneevent", eventClone);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse>();

            return apiResponse;
        }
    }
}

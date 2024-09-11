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
using Omnia.WebContentManagement.Fx.Services;
using Omnia.WebContentManagement.Models.Variations;

namespace Omnia.Migration.Core.Http
{
    public class VariationApiHttpClient : G2HttpClientService, HttpContract.Variation.Interface
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public VariationApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<Variation>> Add(VariationCreationRequest variationCreationRequest)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Variation.Routes.CreateVariation, variationCreationRequest, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Variation>>();

            return await apiResponse;
        }

        public ValueTask<ApiResponse<Variation>> Delete(Variation variationToDelete)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<ApiResponse<IList<Variation>>> Get(Guid? appInstanceId = null)
        {
            var parameters = new NameValueCollection()
                {
                    { "appInstanceId", appInstanceId.ToString() },
                };

            var httpResponse = await GetAsync(HttpContract.Variation.Routes.GetVariation, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<Variation>>>();

            return await apiResponse;
        }

        public ValueTask<ApiResponse<Variation>> Update(Variation updatedVariation)
        {
            throw new NotImplementedException();
        }

        public ValueTask<ApiResponse<Dictionary<Guid, IList<Variation>>>> Get(Guid[] publishingAppIds = null)
        {
            throw new NotImplementedException();
        }
    }
}

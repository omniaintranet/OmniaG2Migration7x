using Microsoft.Extensions.Options;
using Omnia.Fx.MediaPicker.StorageProvider;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.MediaPicker;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using System.Net.Http;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class WcmImageApiHttpClient : G2HttpClientService
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public WcmImageApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings) : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<string> UploadPageImageAsync(string base64, int pageId, string fileName)
        {
            var body = new
            {
                originalImage = new
                {
                    mediaType = 0,
                    base64,
                    fileName
                },
                transformedImage = new
                {
                    mediaType = 0,
                    base64,
                    fileName
                },
                imageAlternateText = "av",
                providerContext = new
                {
                    omniaServiceId = "d60fa82a-129a-41a9-93ce-d784dcb217b0",
                    storageProviderContextId = "d4068218-75b6-4dab-beb0-a96b0b33984d",
                    pageId
                }
            };
            var httpResponse = await PostAsJsonAsync(HttpContract.MediaPickerStorageService.Routes.NewImageAsync, body, parameters: null);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<MediaPickerImage>>();

            return $"{BaseUrl}/{HttpContract.MediaPickerStorageService.Routes.NewImageAsync}/{apiResponse.Data.OmniaImageId}/{apiResponse.Data.FileName}";
        }

        public async ValueTask<MediaPickerImage> UploadChannelImageAsync(string base64, string fileName, string imageAlternateText)
        {
            var body = new
            {
                originalImage = new
                {
                    mediaType = 0,
                    base64,
                    fileName
                },
                transformedImage = new
                {
                    mediaType = 0,
                    base64,
                    fileName,
                    altText = imageAlternateText
                },
                providerContext = new
                {
                    omniaServiceId = "d60fa82a-129a-41a9-93ce-d784dcb217b0",
                    storageProviderContextId = "1de38673-a37d-4a0f-8e31-ae7934163a31"
                }
            };
            var httpResponse = await PostAsJsonAsync(HttpContract.MediaPickerStorageService.Routes.NewImageAsync, body);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<MediaPickerImage>>();

            return apiResponse.Data;
        }
    }
}

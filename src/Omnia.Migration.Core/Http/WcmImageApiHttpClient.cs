using Microsoft.Extensions.Options;
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

        //protected override void EnsureDefaultHeaders(HttpRequestHeaders headers)
        //{
        //    headers.Accept.Clear();
        //    headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    headers.AddOmniaUserAgent();

        //    var tokenKey = MigrationSettings.Value.OmniaTokenKey;
        //    if (!string.IsNullOrEmpty(tokenKey))
        //    {
        //        headers.Add("Cookie", "OmniaTokenKey=" + tokenKey);
        //    }
        //    else
        //    {
        //        headers.Add(Fx.Constants.Parameters.ClientId, MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString());
        //        headers.Add(Fx.Constants.Parameters.ClientSecret, MigrationSettings.Value.OmniaSecuritySettings.ClientSecret);
        //    }
        //}

        public async ValueTask<string> UploadImageAsync(string base64, int pageId, string fileName)
        {
            //fileName = "index&.jpg";
            var body = new
            {
                originalImage = new
                {
                    mediaType = 0,
                    base64 = base64,
                    fileName = fileName
                },
                transformedImage = new
                {
                    mediaType = 0,
                    base64 = base64,
                    fileName = fileName
                },
                imageAlternateText = "av",
                providerContext = new
                {
                    omniaServiceId = "d60fa82a-129a-41a9-93ce-d784dcb217b0",
                    storageProviderContextId = "d4068218-75b6-4dab-beb0-a96b0b33984d",
                    pageId = pageId
                }
            };
            var httpResponse = await PostAsJsonAsync("/api/mediapicker/image", body, parameters: null);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<MediaPickerImage>>();

            //return $"{BaseUrl}/api/mediapicker/image/original/{apiResponse.Data.OmniaImageId}/{apiResponse.Data.FileName}"
            return $"{BaseUrl}/api/mediapicker/image/{apiResponse.Data.OmniaImageId}/{apiResponse.Data.FileName}";
        }
    }
}

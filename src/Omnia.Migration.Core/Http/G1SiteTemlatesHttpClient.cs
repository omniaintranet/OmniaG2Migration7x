using Microsoft.Extensions.Options;
using Omnia.Foundation.Models.Shared;
using Omnia.Foundation.Models.Sites;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.BlockData;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class G1SiteTemplatesHttpClient: G1HttpClientService
    {
        protected override string BaseUrl
        {
            get
            {
                return $"{MigrationSettings.Value.OmniaG1Settings.FoundationUrl}/api/allsitetemplates";
            }
        }

        public G1SiteTemplatesHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
        }

        public async ValueTask<ApiOperationResult<List<SiteTemplate>>> GetSiteTemplates()
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", MigrationSettings.Value.WCMContextSettings.SharePointUrl },
                };

            var httpResponse = await GetAsync("", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<List<SiteTemplate>>>();

            return await apiResponse;
        }
    }
}

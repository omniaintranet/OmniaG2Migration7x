using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Omnia.Migration.Core.Http
{    
    public abstract class G1HttpClientService: BaseHttpClientService
    {
        public G1HttpClientService(
          IHttpClientFactory httpClientFactory,
          IOptionsSnapshot<MigrationSettings> migrationSettings)
          : base(httpClientFactory, migrationSettings)
        {
        }

        protected override void EnsureDefaultHeaders(HttpRequestHeaders headers)
        {
            headers.Accept.Clear();
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(MigrationSettings.Value.OmniaG1Settings.TokenKey))
            {                
                headers.Add("TokenKey", MigrationSettings.Value.OmniaG1Settings.TokenKey);
            }
            else
            {
                headers.Add("ExtensionId", MigrationSettings.Value.OmniaG1Settings.ExtensionId.ToString());
                headers.Add("ApiSecret", MigrationSettings.Value.OmniaG1Settings.ApiSecret);
            }
            
        }
    }
}

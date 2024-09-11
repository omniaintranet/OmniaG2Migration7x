using Microsoft.Extensions.Options;
using Omnia.Fx;
using Omnia.Fx.Models.AppSettings;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Omnia.Migration.Core.Http
{
    public abstract class G2HttpClientService : BaseHttpClientService
    {
        public G2HttpClientService(
           IHttpClientFactory httpClientFactory,
           IOptionsSnapshot<MigrationSettings> migrationSettings)
           : base(httpClientFactory, migrationSettings)
        {
        }

        protected override void EnsureDefaultHeaders(HttpRequestHeaders headers)
        {
            if (MigrationSettings.Value.MigrateCustomLink.MigrateCustomLinktoG2 == false)
            {
                headers.Accept.Clear();
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }                       

            headers.AddOmniaUserAgent();

            var tokenKey = MigrationSettings.Value.OmniaTokenKey;            
            if (!string.IsNullOrEmpty(tokenKey))
            {
                headers.Add("Cookie",  "OmniaTokenKey=" + tokenKey);
            }
            else
            {
                headers.Add(Omnia.Fx.Constants.Parameters.ClientId, MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString());
                headers.Add(Omnia.Fx.Constants.Parameters.ClientSecret, MigrationSettings.Value.OmniaSecuritySettings.ClientSecret);
            }
        }
    }
}

using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.EnterpriseProperties;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Fx.Models.Shared;
using Omnia.Fx.SharePoint.Fields.BuiltIn;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class IdentityApiHttpClient : G2HttpClientService
    {// Omnia.Fx.Identities.HttpContract.IdentityService.Routes.Query
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(new Guid("bb000000-0000-bbbb-0000-0000000000bb"));
            }
        }

        public IdentityApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<ItemQueryResult<IResolvedIdentity>>> GetUserIdentitybyEmail(string email)
        {
            IdentityQuery para = new IdentityQuery()
            {
                ExcludeTypeIds = new List<Guid> { new Guid("930fd165-96a9-4503-8d14-f7369694c75f") },
                ItemLimit = email.Length == 0 ? 50000 : 10,
                ProviderIds = new List<Guid> { new Guid("bb9f80dd-9dfa-4147-b923-7e2f8e0e7a0c") },
                SearchText = email,
                Type = IdentityTypes.User 
            };
            
            var parameters = new NameValueCollection()
                {
                    
                };

            var httpResponse = await PostAsJsonAsync(Omnia.Fx.Identities.HttpContract.IdentityService.Routes.Query+ "?omniaapp=false", para, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<ItemQueryResult<IResolvedIdentity>>>();

            return await apiResponse;
        }
    }
}

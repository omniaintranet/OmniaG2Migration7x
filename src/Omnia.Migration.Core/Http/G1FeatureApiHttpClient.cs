using FeaturesAction.Features;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Omnia.Foundation.Models.Features;
using Omnia.Foundation.Models.Shared;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Http
{
    public class G1FeatureApiHttpClient : G1HttpClientService
    {
        protected override string BaseUrl
        {
            get
            {
                return $"{MigrationSettings.Value.OmniaG1Settings.FoundationUrl}/api/feature";
            }
        }

        public G1FeatureApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {            
        }

        public async ValueTask<ApiOperationResult<FeatureModel>> GetFeatureInfoAsync(Guid featureId, string spUrl)
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", spUrl }
                };

            var httpResponse = await GetAsync("/" + featureId.ToString(), parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<FeatureModel>>();
            
            return await apiResponse;
        }

        public ActivateResult DeactivateFeatureAsync(Guid featureId, string spUrl)
        {
            string deativateApi= "{0}/api/feature/{1}/deactivate?ContentType=application%2Fjson&Lang=en-us";
            string hostApi = string.Format(deativateApi,MigrationSettings.Value.OmniaG1Settings.FoundationUrl,featureId);

            var parameters = new NameValueCollection()
                {
                    { "SPUrl", spUrl },
                    //{ "Force", "true" },
                    //{"TokenKey",MigrationSettings.Value.OmniaG1Settings.TokenKey },
                    {"ContentType","application/json; charset=utf-8"},
                    { "Accept","application/json"},
                    {"TenantId", MigrationSettings.Value.OmniaG1Settings.TenantId},
                    {"ApiSecret",MigrationSettings.Value.OmniaG1Settings.ApiSecret },
                    {"IsAppContext","true" }
                };

            //var httpResponse = await PostAsJsonAsync(hostApi, (object)null,parameters:parameters);
            //var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<FeatureInstance>>();

            //return await apiResponse;

            ActivateResult ret = CreatePostRequestWithHeaders<ActivateResult>(hostApi, null, parameters);
            return ret;
        }
        public T CreatePostRequestWithHeaders<T>(string url, object value, NameValueCollection headers)
        {
            try
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json; charset=utf-8";
                foreach (string header in headers)
                {
                    httpWebRequest.Headers.Add(header, headers[header]);
                }
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string jsonStr = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    streamWriter.Write(jsonStr);
                    streamWriter.Flush();
                    streamWriter.Close();

                    HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string resultStr = streamReader.ReadToEnd();
                        return JsonConvert.DeserializeObject<T>(resultStr);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public async ValueTask<ApiOperationResult<FeatureInstance>> ActivateFeatureAsync(Guid featureId, string spUrl)
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", spUrl },
                    { "Force", "true" }
                };

            var httpResponse = await PostAsJsonAsync($"/{featureId.ToString()}/activate", (object)null, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<FeatureInstance>>();

            return await apiResponse;
        }

        public async ValueTask<ApiOperationResult<FeatureInstanceStatus>> GetFeatureInstanceStatusAsnyc(Guid featureId, string targetUrl)
        {
            var parameters = new NameValueCollection()
                {
                    { "SPUrl", targetUrl },
                    { "targetUrl", targetUrl }
                };

            var httpResponse = await GetAsync($"/{featureId.ToString()}/activate/status", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiOperationResult<FeatureInstanceStatus>>();

            return await apiResponse;
        }
    }
}

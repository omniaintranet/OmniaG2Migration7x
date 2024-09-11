using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Omnia.Fx.Utilities;
using Omnia.Fx;
using Omnia.Migration.Models.Configuration;
using Microsoft.Extensions.Options;

namespace Omnia.Migration.Core.Http
{
    public abstract class BaseHttpClientService
    {
        protected abstract string BaseUrl { get; }
        protected HttpClient HttpClient { get; }
        protected IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public BaseHttpClientService(IHttpClientFactory httpClientFactory, IOptionsSnapshot<MigrationSettings> migrationSettings)
        {            
            HttpClient = httpClientFactory.CreateClient("omnia");
            MigrationSettings = migrationSettings;
        }

        protected virtual HttpRequestMessage CreateRequest(string requestUri, HttpMethod method, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = GetRequestUri(requestUri, parameters),
                Method = method
            };

            EnsureHeaders(request.Headers, headers);

            if (overrideHeaderOptions == null)
                EnsureDefaultHeaders(request.Headers);
            else
                overrideHeaderOptions.Invoke(request.Headers);

           

            return request;
        }

        protected abstract void EnsureDefaultHeaders(HttpRequestHeaders headers);       

        protected virtual void EnsureHeaders(HttpRequestHeaders headers, NameValueCollection headerParams)
        {
            if (headerParams == null || headerParams.Count == 0)
                return;

            foreach (string key in headerParams)
            {
                if (!headers.Contains(key))
                {
                    headers.Add(key, headerParams[key]);
                }
            }
        }

        protected virtual void EnsureCookies(HttpRequestMessage request, NameValueCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
                return;

            foreach (string cookieName in cookies)
            {
                request.Headers.Add("Cookie", $"{cookieName}={cookies[cookieName]}");
            }
        }

        protected virtual StringContent CreateBodyStringContent<T>(T value)
        {
            var content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
            return content;
        }

        protected Uri GetRequestUri(string requestUri, NameValueCollection parameters = null)
        {            
            var paramStr = UrlUtils.BuildQuerystringParameters(parameters);
            var requestUrl = requestUri;
            if (!string.IsNullOrWhiteSpace(BaseUrl))
            {
                requestUrl = requestUrl.TrimStart('/');
                requestUrl = $"{this.BaseUrl}/{requestUrl}";
            }
            return new Uri($"{requestUrl}{paramStr}");
        }
        

        protected virtual HttpResponseMessage Send(HttpRequestMessage request)
        {
            return SendAsync(request)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await HttpClient.SendAsync(request);
        }

        protected virtual HttpResponseMessage Get(string requestUri, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return GetAsync(requestUri, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> GetAsync(string requestUri, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var request = CreateRequest(requestUri, HttpMethod.Get, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);
            return await SendAsync(request);
        }

        protected virtual HttpResponseMessage PostAsJson<T>(string requestUri, T value, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return PostAsJsonAsync(requestUri, value, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var postContent = CreateBodyStringContent(value);
            return await PostAsync(requestUri, postContent, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);
        }

        protected virtual HttpResponseMessage Post(string requestUri, HttpContent content, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return PostAsync(requestUri, content, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var request = CreateRequest(requestUri, HttpMethod.Post, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);
            request.Content = content;

            return await SendAsync(request);
        }

        protected virtual HttpResponseMessage PutAsJson<T>(string requestUri, T value, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return PutAsJsonAsync(requestUri, value, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var putContent = CreateBodyStringContent(value);
            return await PutAsync(requestUri, putContent, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);
        }

        protected virtual HttpResponseMessage Put(string requestUri, HttpContent content, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return PutAsync(requestUri, content, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var request = CreateRequest(requestUri, HttpMethod.Put, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);
            request.Content = content;

            return await SendAsync(request);
        }

        protected virtual HttpResponseMessage Delete(string requestUri, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            return DeleteAsync(requestUri, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        protected virtual async ValueTask<HttpResponseMessage> DeleteAsync(string requestUri, NameValueCollection parameters = null, NameValueCollection headers = null, Action<HttpRequestHeaders> overrideHeaderOptions = null)
        {
            var request = CreateRequest(requestUri, HttpMethod.Delete, parameters: parameters, headers: headers, overrideHeaderOptions: overrideHeaderOptions);

            return await SendAsync(request);
        }
    }
}

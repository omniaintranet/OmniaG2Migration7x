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
using Omnia.WebContentManagement.Models.Social;

namespace Omnia.Migration.Core.Http
{
    public class SocialApiHttpClient : G2HttpClientService, HttpContract.PageSocial.SocialInterface
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public SocialApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<Omnia.Fx.Models.Social.Comment>> AddComment(Omnia.Fx.Models.Social.Comment comment)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { Fx.Constants.Parameters.IsSystemUpdate, "true" }
                };
            var httpResponse = await PostAsJsonAsync(HttpContract.PageSocial.SocialRoutes.AddComment, comment, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Omnia.Fx.Models.Social.Comment>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse> AddOrUpdateLike(string topicId, string commentId, bool isLike, string loginName = null)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "topicId", topicId },
                    { "commentId", commentId },
                    { "isLike", isLike.ToString() },
                    { "loginName", loginName },
                    {  Fx.Constants.Parameters.IsSystemUpdate, "true" }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.PageSocial.SocialRoutes.AddOrUpdateLike, (string) null, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Comment>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Omnia.Fx.Models.Social.Comment>> UpdateComment(Omnia.Fx.Models.Social.Comment comment)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<Fx.Models.Social.CommentLike>> HttpContract.PageSocial.SocialInterface.GetTopic(string topicId)
        {
            throw new NotImplementedException();
        }
    }
}

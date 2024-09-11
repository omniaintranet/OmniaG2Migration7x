using Omnia.Fx.Models.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Omnia.WebContentManagement.Fx.Services;
using Omnia.WebContentManagement.Models.Pages;
using System.Collections.Specialized;
using Omnia.WebContentManagement.Models.Pages.HttpContractModels;
using Omnia.WebContentManagement.Models.Layout;
using Omnia.WebContentManagement.Models.PublishingApp;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Migration.Models.Configuration;
using Omnia.WebContentManagement.Models.Navigation;
using Newtonsoft.Json;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Models.Input.MigrationItem;

namespace Omnia.Migration.Core.Http
{
    public class CreatedPageWithNavigationInfo<T> where T : PageData
    {
        public CheckedOutVersionPageData<T> CheckedOutVersion { get; set; }
        public PageNavigationNode<PageNavigationData> PageNavigationNode { get; set; }
    }

    public class CreatedPageInfo
    {
        public CheckedOutVersionPageData<PageData> CheckedOutVersion { get; set; }
        public Page Page { get; set; }
    }

    public class PageApiHttpClient : G2HttpClientService//, HttpContract.Page.Interface
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl {
            get {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public PageApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<VersionedPageData<PageData>>> CheckInAsync(CheckedOutVersionPageData<PageData> versionToCheckIn)
        {
            var parameters = GetDefaultParameters();            

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.CheckIn, versionToCheckIn, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<VersionedPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<CheckedOutVersionPageData<PageData>>> CheckOutAsync(int pageId)
        {
            return await CheckOutAsync(MigrationSettings.Value.WCMContextSettings.PublishingAppId, pageId);
        }

        public async ValueTask<ApiResponse<CheckedOutVersionPageData<PageData>>> CheckOutAsync(Guid publishingAppId, int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await PostAsync(HttpContract.Page.Routes.CheckOut, null, parameters: parameters);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<CheckedOutVersionPageData<PageData>>>();
            //var raw = await httpResponse.Content.ReadAsStringAsync();
            //var apiResponse2 = JsonConvert.DeserializeObject<ApiResponse<CheckedOutVersionPageData<PageData>>>(raw);

            //var temp = JsonConvert.SerializeObject(apiResponse2);

            return apiResponse;
        }

        public async ValueTask<ApiResponse<CreatedPageInfo>> CreateVariationPage(VariationPageCreationRequest pageCreationInfo)
        {
            string strResponse = string.Empty;

            try
            {
                var parameters = GetDefaultParameters();

                var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.CreateVariationPage, pageCreationInfo, parameters: parameters);
                strResponse = await httpResponse.Content.ReadAsStringAsync();

                var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<CreatedPageInfo>>();

                return await apiResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error when creating page. Response: " + strResponse + ". Error message: " + ex.Message + ex.StackTrace);                
            }
        }

        public async ValueTask<ApiResponse<CreatedPageWithNavigationInfo<PageData>>> CreatePage(PageWithNavigationCreationRequest pageCreationInfo)
        {
            string strResponse = string.Empty;

            try
            {
                var parameters = GetDefaultParameters();

                var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.CreateWithNavigation, pageCreationInfo, parameters: parameters);
                strResponse = await httpResponse.Content.ReadAsStringAsync();

                var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<CreatedPageWithNavigationInfo<PageData>>>();

                return await apiResponse;
            }
            catch (Exception ex)
            {
                throw new Exception("Error when creating page. Response: " + strResponse + ". Error message: " + ex.Message + ex.StackTrace);
            }
        }

        public async ValueTask<ApiResponse> DeletePageAsync(int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await DeleteAsync(HttpContract.Page.Routes.CheckOut, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<IList<Page>>> GetAllPagesAsync(int typesToGet = -1)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("typesToGet", typesToGet.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetAllPages, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<Page>>>();

            return await apiResponse;
        }
        
        // The request was deprecated
        /*
        public async ValueTask<ApiResponse<IList<IVersionedPageData>>> GetAllVersionsAsync(int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetAllVersions, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<IVersionedPageData>>>();

            return await apiResponse;
        }*/

        public async ValueTask<ApiResponse<CheckedOutVersionPageData<PageData>>> GetCheckedOutVersionAsync(int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetCheckedOutVersion, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<CheckedOutVersionPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Page>> GetPage(int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetPage, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Page>>();

            return await apiResponse;
        }
        public async ValueTask<ApiResponse<PageNavigationMigrationItem>> GetPageByVersion(int versionId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("versionId", versionId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetVersion, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<PageNavigationMigrationItem>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Page>> GetPageAsync(VersionedPageData<PageData> version)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.GetPageByVersion, version, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Page>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Dictionary<int, Page>>> GetPagesAsync(int[] pageIds)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.GetPagesByIds, pageIds, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Dictionary<int, Page>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<PublishedVersionPageData<PageData>>> GetPublishedVersionAsync(int pageId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("pageId", pageId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetPublishedVersion, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<PublishedVersionPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<IVersionedPageData>> GetVersionAsync(int versionId)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("versionId", versionId.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.GetVersion, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IVersionedPageData>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<MergedPageVersionLayout>> MergeVersionLayoutsAsync(int versionId, int? parentLayoutVersionId = null)
        {
            var parameters = GetDefaultParameters();
            parameters.Add("versionId", versionId.ToString());
            parameters.Add("parentLayoutVersionId", parentLayoutVersionId?.ToString());

            var httpResponse = await GetAsync(HttpContract.Page.Routes.BaseRoute + "/merge", parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<MergedPageVersionLayout>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<PublishedVersionPageData<PageData>>> PublishAsync(VersionedPageData<PageData> versionToPublish)
        {
            return await PublishAsync(MigrationSettings.Value.WCMContextSettings.PublishingAppId, versionToPublish);
        }

        public async ValueTask<ApiResponse<PublishedVersionPageData<PageData>>> PublishAsync(Guid publishingAppId, VersionedPageData<PageData> versionToPublish)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.Publish, versionToPublish, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<PublishedVersionPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<CheckedOutVersionPageData<PageData>>> SaveSateAsync(CheckedOutVersionPageData<PageData> versionToSaveSateFor)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.SaveState, versionToSaveSateFor, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<CheckedOutVersionPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<VersionedPageData<PageData>>> UndoCheckOutAsync(CheckedOutVersionPageData<PageData> versionToUndo)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.UndoCheckOut, versionToUndo, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<VersionedPageData<PageData>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<VersionedPageData<PageData>>> UnPublishAllAsync(PublishedVersionPageData<PageData> publishedVersionToUnpublish)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.Unpublish, publishedVersionToUnpublish, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<VersionedPageData<PageData>>>();

            return await apiResponse;
        }

        //public async ValueTask<ApiResponse> UpdatePagePermissions(int pageId, PagePermissionInfo permissionInfo)
        //{
        //    var parameters = new NameValueCollection()
        //        {
        //            { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
        //            { "pageId", pageId.ToString() }
        //        };

        //    var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.UpdatePagePermissions, permissionInfo, parameters: parameters);
        //    var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse>();

        //    return await apiResponse;
        //}        
        /*
        public async ValueTask<ApiResponse<VariationPage>> AddVariation( addVariationRequest)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.AddVariation, addVariationRequest, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<VariationPage>>();

            return await apiResponse;
        }*/

        public async ValueTask<ApiResponse<IList<VariationPage>>> GetVariations(Page page)
        {
            var parameters = GetDefaultParameters();

            var httpResponse = await PostAsJsonAsync(HttpContract.Page.Routes.GetVariations, page, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<VariationPage>>>();

            return await apiResponse;
        }

        private NameValueCollection GetDefaultParameters()
        {
            var parameters = new NameValueCollection()
                {
                    { "appInstanceId", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            //if (MigrationSettings.Value.WCMContextSettings.DefaultVariationId != 0)
            //{
            //    parameters.Add("variationid", MigrationSettings.Value.WCMContextSettings.DefaultVariationId.ToString());
            //}

            return parameters;
        }
    }
}

using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Omnia.WebContentManagement.Fx.Services;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Navigation.HttpContractModels;
using Omnia.WebContentManagement.Models.Pages;

namespace Omnia.Migration.Core.Http
{
    public class NavigationApiHttpClient : G2HttpClientService, HttpContract.Navigation.Interface
    {
        private IOptionsSnapshot<OmniaServicesDnsSettings> OmniaServiceDnsSettings { get; }

        protected override string BaseUrl
        {
            get
            {
                return OmniaServiceDnsSettings.Value.GetServiceDns(WebContentManagement.Fx.Constants.WCMServices.WebApp.Id);
            }
        }

        public NavigationApiHttpClient(
            IHttpClientFactory httpClientFactory,
            IOptionsSnapshot<OmniaServicesDnsSettings> omniaServiceDnsSettings,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
            : base(httpClientFactory, migrationSettings)
        {
            OmniaServiceDnsSettings = omniaServiceDnsSettings;
        }

        public async ValueTask<ApiResponse<INavigationNode>> CreateAsync(CreateNavigationRequest request)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };
            var headers = new NameValueCollection()
                {
                    { "Cookie", MigrationSettings.Value.MigrateCustomLink.Cookie },
                    { "Accept",MigrationSettings.Value.MigrateCustomLink.Accept }
                };
            MigrationSettings.Value.MigrateCustomLink.MigrateCustomLinktoG2 = true;
            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.CreatenNode, request, parameters: parameters, headers: headers);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<INavigationNode>>();


            return await apiResponse;
        }

        public async ValueTask<ApiResponse<INavigationNode>> UpdateNodeDataAsync(INavigationNode nodeToUpdate)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.UpdateNode, nodeToUpdate, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<INavigationNode>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<MoveNavigationNodeResult>> MoveAsync(MoveRequest request)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.MoveNode, request, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<MoveNavigationNodeResult>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<IList<INavigationNode>>> GetParentNodesAsync(NavigationNode<NavigationData> node, int nodeTypes = -1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "nodeTypes", nodeTypes.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.GetParentNodes, node, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<INavigationNode>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<IList<INavigationNode>>> GetChildrenAsync(INavigationNode node, int nodeTypes = -1, int depth = 1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "nodeTypes", nodeTypes.ToString() },
                    { "depth", depth.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.GetChildrenNodes, node, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<INavigationNode>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<IList<INavigationNode>>> GetChildrenAsync(Guid publishingAppId, NavigationNode<NavigationData> node, int nodeTypes = -1, int depth = 1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", publishingAppId.ToString() },
                    { "nodeTypes", nodeTypes.ToString() },
                    { "depth", depth.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.GetChildrenNodes, node, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<IList<INavigationNode>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Dictionary<int, INavigationNode>>> GetNodesById(int[] nodeIds)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.GetNodesById, nodeIds, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<Dictionary<int, INavigationNode>>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<NavigationResult>> GetNavigationTreeAsync(string path, int nodeTypes = -1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "path", path },
                    { "nodeTypes", nodeTypes.ToString() }
                };

            var httpResponse = await GetAsync(HttpContract.Navigation.Routes.GetNavigationResultByPath, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<NavigationResult>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<NavigationResult>> GetNavigationTreeAsync(int nodeId, int nodeTypes = -1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "nodeId", nodeId.ToString() },
                    { "nodeTypes", nodeTypes.ToString() }
                };

            var httpResponse = await GetAsync(HttpContract.Navigation.Routes.GetNavigationResultById, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<NavigationResult>>();

            return await apiResponse;
        }

        public async ValueTask<ApiResponse<Dictionary<PageId, IList<IPageNavigationNode>>>> GetPageNavigationNodesAsync(PageId[] pageIds, int nodeTypes = -1)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() },
                    { "nodeTypes", nodeTypes.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.GetPageNavigationNodesById, pageIds, parameters: parameters);
            var apiResponse = await httpResponse.Content.ReadAsJsonAsync<ApiResponse<Dictionary<PageId, IList<IPageNavigationNode>>>>();

            return apiResponse;
        }

        //Deprecated
        /*
        public async ValueTask<ApiResponse<DeleteNavigationNodeResult>> DeleteNavigationNodesAsync(int[] nodeIdsToDelete)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            var httpResponse = await PostAsJsonAsync(HttpContract.Navigation.Routes.DeleteNodes, nodeIdsToDelete, parameters: parameters);
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse<DeleteNavigationNodeResult>>();

            return await apiResponse;
        }*/

        public ValueTask<ApiResponse<IList<INavigationNode>>> GetParentNodesAsync(INavigationNode node, int nodeTypes = -1)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<ApiResponse> DeleteLinkNavigationNodeAsync(int nodeIdsToDelete)
        {
            var parameters = new NameValueCollection()
                {
                    { "publishingappid", MigrationSettings.Value.WCMContextSettings.PublishingAppId.ToString() }
                };

            var httpResponse = await DeleteAsync($"{HttpContract.Navigation.Routes.DeleteLinkNode}/{nodeIdsToDelete}");
            var apiResponse = httpResponse.Content.ReadAsJsonAsync<ApiResponse>();

            return await apiResponse;
        }

        ValueTask<ApiResponse<MovePageCollectionResult>> HttpContract.Navigation.Interface.MovePageCollectionAsync(MovePageCollRequest request)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<INavigationNode>> HttpContract.Navigation.Interface.CreateAsync(CreateNavigationRequest request)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<INavigationNode>> HttpContract.Navigation.Interface.UpdateNodeDataAsync(INavigationNode nodeToUpdate)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<MoveNavigationNodeResult>> HttpContract.Navigation.Interface.MoveAsync(MoveRequest request)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<MovePageCollectionValidationResult>> HttpContract.Navigation.Interface.ValidateMovingPageCollectionAsync(MovePageCollRequest request)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<IList<INavigationNode>>> HttpContract.Navigation.Interface.GetParentNodesAsync(INavigationNode node, int nodeTypes)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<IList<INavigationNode>>> HttpContract.Navigation.Interface.GetChildrenAsync(INavigationNode node, int nodeTypes, int depth)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<Dictionary<int, INavigationNode>>> HttpContract.Navigation.Interface.GetNodesById(int[] nodeIds)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<NavigationResult>> HttpContract.Navigation.Interface.GetNavigationTreeAsync(string path, int nodeTypes)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<NavigationResult>> HttpContract.Navigation.Interface.GetNavigationTreeAsync(int nodeId, int nodeTypes)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse<Dictionary<PageId, IList<IPageNavigationNode>>>> HttpContract.Navigation.Interface.GetPageNavigationNodesAsync(PageId[] pageIds, int nodeTypes)
        {
            throw new NotImplementedException();
        }

        ValueTask<ApiResponse> HttpContract.Navigation.Interface.DeleteLinkNavigationNodeAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}

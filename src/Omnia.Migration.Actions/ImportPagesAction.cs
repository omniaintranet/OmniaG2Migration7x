using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Shared;
using Omnia.Fx.Utilities;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Core.Reports;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Navigation.HttpContractModels;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Pages.HttpContractModels;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Omnia.Migration.Models.Shared;
using Omnia.Fx.Contexts.Scoped;
using Omnia.Fx.MultilingualTexts;
using Omnia.WebContentManagement.Models.Utils;
using Omnia.Migration.Models.LegacyWCM;
using DocumentFormat.OpenXml.Bibliography;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.Fx.SharePoint.Fields.BuiltIn;

namespace Omnia.Migration.Actions
{
    public class ImportPagesAction : ParallelizableMigrationAction
    {
        private PageApiHttpClient PageApiHttpClient { get; }
        private NavigationApiHttpClient NavigationApiHttpClient { get; }
        private VariationApiHttpClient VariationApiHttpClient { get; }
        private SocialService SocialService { get; }
        private ImagesService ImagesService { get; }
        private PagesService PagesService { get; }
        private WcmService WcmService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        //ILogger<ImportPagesAction> Logger { get; }
        private IProgressManager ProgressManager { get; set; }
        private WcmBaseData WcmData { get; set; }
        private Dictionary<Guid, string> PageIdMapping { get; set; }
        private IdentityApiHttpClient IdentityApiHttpClient { get; }
        public ItemQueryResult<IResolvedIdentity> Identities { get; set; }
        public IList<IResolvedIdentity> da { get; set; }


        public ImportPagesAction(
            PageApiHttpClient pageApiHttpClient,
            NavigationApiHttpClient navigationApiHttpClient,
            VariationApiHttpClient variationApiHttpClient,
            SocialService socialService,
            ImagesService imagesService,
            IdentityApiHttpClient identityApiHttpClient,
            PagesService pagesService,
            WcmService wcmService,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            PageApiHttpClient = pageApiHttpClient;
            NavigationApiHttpClient = navigationApiHttpClient;

            VariationApiHttpClient = variationApiHttpClient;
            ImagesService = imagesService;
            SocialService = socialService;
            PagesService = pagesService;
            WcmService = wcmService;
            MigrationSettings = migrationSettings;
            PageIdMapping = new Dictionary<Guid, string>();
            IdentityApiHttpClient = identityApiHttpClient;
        }

        public override async Task StartAsync(IProgressManager progressManager)
        {
            ProgressManager = progressManager;
            List<NavigationMigrationItem> input = ReadInput();
            

            Console.WriteLine("Select input file to run:....");
            Console.WriteLine("     1. Run for all data in json file");
            Console.WriteLine("     2. Run for only data in filter file");
            var filteroption = Console.ReadLine();
            if (filteroption == "2")
            {
                input = FilterInput(input);
            }
            // IEnumerable<string> m_oEnum = new string[] { "c-ooredsson@swep.net" };
            // var usersun = await IdentityApiHttpClient.ResolveUserIdentitiesWithEmailsAsync(m_oEnum);

           
            // Thoan modified 7.6 changed API get user by paging 5000
            var user2 = await IdentityApiHttpClient.GetUserall(1, 5000);
            if (user2 == null || user2.Data.Total == 0)
            {
                throw new Exception("Can not get Identities Please check again");
               // Console.WriteLine("Can not get Identities Please check again");

            }
            var userall = new List<ResolvedUserIdentity>();
            userall = user2.Data.Value.ToList();            

            int totalnumber = user2.Data.Total;

            int pagetotal = totalnumber / 5000;
            if (pagetotal == 1)
            {
                var user6 = await IdentityApiHttpClient.GetUserall(2, 5000);
                userall.AddRange(user6.Data.Value);
                Console.WriteLine("Resolved " + (user6.Data.Value.Count() + 5000).ToString());

            }
            if (pagetotal > 1)            {
                for (int i = 2; i <= pagetotal+1; i++)
                {
                    var user6 = await IdentityApiHttpClient.GetUserall(i, 5000);
                    userall.AddRange(user6.Data.Value);
                    Console.WriteLine("Resolved " + (i * 5000).ToString());

                }
            }
            Console.WriteLine("Resolved done");

            IList<IResolvedIdentity> s = userall.Cast<IResolvedIdentity>().ToList();
            var a = new ItemQueryResult<IResolvedIdentity>();
            a.Items = s;
            this.Identities = a;


            ProgressManager.Start(input.GetTotalCount());
            ImportPagesReport.Instance.Init(MigrationSettings.Value);

            try
            {
                WcmData = await WcmService.LoadWcmBaseDataAsync();

                WcmService.EnsureAndValidateWcmSettings(WcmData);

                RunInParallel(input, MigrationSettings.Value.ImportPagesSettings.NumberOfParallelThreads, async (partitionedInput) =>
                {
                    await ImportAsync(partitionedInput, WcmData.PageCollectionNode);
                });
            }
            catch (Exception ex)
            {
                ImportPagesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
                //Logger.LogError(ex.Message + ex.StackTrace);
                Logger.Log(ex.Message + ex.StackTrace);
            }
            finally
            {
                ImportPagesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
            }
        }
        private List<NavigationMigrationItem> FilterInput(List<NavigationMigrationItem> input)
        {
            List<NavigationMigrationItem> filterInput = new List<NavigationMigrationItem>();
            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportPagesSettings.InputFilterFile);
            string[] lines = System.IO.File.ReadAllLines(@inputPath);
            foreach (string line in lines)
            {
                var filter = input.Where(x => x.AdditionalProperties["UrlSegment"].ToString() == line.ToString()).ToList();
                filterInput.AddRange(filter);
            }
            return filterInput;
        }
        private List<NavigationMigrationItem> ReadInput()
        {
            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportPagesSettings.InputFile);
            var inputStr = File.ReadAllText(inputPath);
            if (!inputStr.StartsWith("[") && !inputStr.EndsWith("]"))
            {
                inputStr = "[" + inputStr + "]";
            }

            var input = JsonConvert.DeserializeObject<List<NavigationMigrationItem>>(inputStr);

            // If the input has one root node then it's the page collection. The tool will not migrate the page collection itself.
            if (input.Count == 1)
                input = input.First().Children;

            return input;
        }

        private async Task ImportAsync(List<NavigationMigrationItem> navigationNodes, INavigationNode parentNode)
        {
            foreach (var node in navigationNodes)
            {
                ApiResponse<INavigationNode> migrationResult = null;

                switch (node.MigrationItemType)
                {
                    case NavigationMigrationItemTypes.Link:
                        var linkNode = CloneHelper.CloneToLinkMigration(node);
                        migrationResult = await ImportNavigationLinkAsync(linkNode, parentNode);
                        break;
                    case NavigationMigrationItemTypes.Page:
                        var pageNode = CloneHelper.CloneToPageMigration(node);
                        //pageNode.AdditionalProperties = node.AdditionalProperties;
                        //pageNode.ShowInCurrentNavigation = node.ShowInCurrentNavigation;
                        //pageNode.ParentId = node.ParentId;
                        //pageNode.Children = node.Children;
                        //pageNode.ShowInMegaMenu = node.ShowInMegaMenu;
                        migrationResult = await ImportNavigationPageAsync(pageNode, parentNode);
                        break;
                    default:
                        break;
                }
            }
        }

        private async ValueTask<ApiResponse<INavigationNode>> ImportNavigationLinkAsync(LinkNavigationMigrationItem migrationItem, INavigationNode parentNode)
        {
            //Hieu rem
            //INavigationNode linkNode = null;
            INavigationNode linkNode = null;

            try
            {
                migrationItem.Url = UrlHelper.MapUrl(migrationItem.Url,
                    MigrationSettings.Value.WCMContextSettings.SharePointUrl,
                    MigrationSettings.Value.WCMContextSettings.SharePointLocationMappings);

                migrationItem.ShowInCurrentNavigation = true;
                migrationItem.ShowInMegaMenu = true;

                var existingLink = NavigationNodeHelper.FindMatchingNavigationNode(migrationItem, WcmData.ExistingNodes, parentNode);
                if (existingLink != null)
                {
                    var getLinkResult = await NavigationApiHttpClient.GetNodesById(new int[] { existingLink.Id });
                    getLinkResult.EnsureSuccessCode();

                    linkNode = getLinkResult.Data[existingLink.Id];
                    linkNode.IsNullThrow(new Exception("Cannot find existing link with ID " + existingLink.Id));
                    linkNode.NavigationData.AdditionalProperties["url"] = migrationItem.Url;

                    var variationId = WcmData.DefaultVariation != null ? (int)WcmData.DefaultVariation.Id : 0;
                    //Hieu added
                    if (linkNode.NavigationNodeType is NavigationNodeType.PageCollection)
                    {
                        var pageCollectionNavigationData = (PageCollectionNavigationData)linkNode.NavigationData;
                        var title = migrationItem.Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    else if (linkNode.NavigationNodeType is NavigationNodeType.Page)
                    {
                        var pageCollectionNavigationData = (PlainPageNavigationData)linkNode.NavigationData;
                        var title = migrationItem.Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    //<<
                    //Hieu rem
                    //linkNode.NavigationData.Title.AddOrUpdate(variationId, migrationItem.Title);

                    await NavigationApiHttpClient.UpdateNodeDataAsync(linkNode);
                }
                else
                {
                    var linkCreationRequest = CreateNavigationCreationRequest(migrationItem, parentNode);
                    var linkCreationResult = await NavigationApiHttpClient.CreateAsync(linkCreationRequest);
                    linkCreationResult.EnsureSuccessCode();

                    linkNode = linkCreationResult.Data;
                }

                // TODO:
                // Custom links always show in mega menu and current navigation. 
                // Need to wait for WCM to update before we can fix this

                // We assume that link node has no children for now.
                //await ImportAsync(linkNode., linkNode);

                ImportPagesReport.Instance.AddSucceedItem(migrationItem, linkNode.Id);

                ProgressManager.ReportProgress(1);

                return ApiUtils.CreateSuccessResponse(linkNode);

            }
            catch (Exception ex)
            {
                ImportPagesReport.Instance.AddFailedItem(migrationItem, linkNode != null ? linkNode.Id : 0, ex);
                return ApiUtils.CreateErrorResponse<INavigationNode>(ex);
            }
        }

        private async ValueTask<ApiResponse<INavigationNode>> ImportNavigationPageAsync(PageNavigationMigrationItem migrationItem, INavigationNode parentNode)
        {
            string pagePath = migrationItem.UrlSegment;
            PageId pageId = 0;
            PageNavigationNode<PageNavigationData> pageNode = null;

            try
            {
                migrationItem.PageData.IsNullThrow(new Exception("PageData cannot be null"));

                PageDataMapper.MapPageData(migrationItem, WcmData.PageTypes, MigrationSettings.Value, Identities);

                var existingPage = NavigationNodeHelper.FindMatchingNavigationNode(migrationItem, WcmData.ExistingNodes, parentNode);
                if (existingPage != null)
                {
                    pageNode = await UpdatePageAsync(migrationItem, existingPage);
                }
                else
                {
                    pageNode = await AddNewPageAsync(migrationItem, parentNode);
                    ImportPagesReport.Instance.AddNewItem(migrationItem, pageNode.Id, pageId, pagePath, migrationItem.PhysicalPageUniqueId);
                }

                pageId = pageNode.Page.Id;
                pagePath = pageNode.Path ?? pagePath;

                await ImportTranslationPagesAsync(originalPage: pageNode.Page, translationPages: migrationItem.TranslationPages);
                await SocialService.ImportCommentsAndLikesAsync(pageId, migrationItem, existingPage, Identities);

                ImportPagesReport.Instance.AddSucceedItem(migrationItem, pageNode.Id, pageId, pagePath, migrationItem.PhysicalPageUniqueId);

                ProgressManager.ReportProgress(1);

                await ImportAsync(migrationItem.Children, pageNode);

                return ApiUtils.CreateSuccessResponse(pageNode as INavigationNode);
            }
            catch (Exception ex)
            {
                ImportPagesReport.Instance.AddFailedItem(migrationItem, pageNode != null ? pageNode.Id : 0, pageId, pagePath, ex);
                return ApiUtils.CreateErrorResponse<INavigationNode>(ex);
            }
        }

        private async ValueTask<PageNavigationNode<PageNavigationData>> AddNewPageAsync(PageNavigationMigrationItem migrationItem, INavigationNode parentNode)
        {

            var pageCreationRequest = CreatePageWithNavigationCreationRequest(migrationItem, parentNode);
            var pageCreationResult = await PageApiHttpClient.CreatePage(pageCreationRequest);
            pageCreationResult.EnsureSuccessCode();

            var pageNode = pageCreationResult.Data.PageNavigationNode;
            var pageCheckedOutVersion = pageCreationResult.Data.CheckedOutVersion;
            var pageId = pageNode.Page.Id;

            bool needToUpdateNavigationData = false;
            if (!migrationItem.ShowInMegaMenu || !migrationItem.ShowInCurrentNavigation)
            {
                pageNode.NavigationData.HideInMegaMenu = !migrationItem.ShowInMegaMenu;
                pageNode.NavigationData.HideInCurrentNavigation = !migrationItem.ShowInCurrentNavigation;
                needToUpdateNavigationData = true;
            }

            if (migrationItem.TranslationPages.Count > 0 && MigrationSettings.Value.ImportPagesSettings.ImportTranslationPages == true)
            {
                foreach (var item in migrationItem.TranslationPages)
                {
                    var variationId = (VariationId)MigrationSettings.Value.WCMContextSettings.VariationMappings[item.PageLanguage];

                    //Hieu added
                    if (pageNode.NavigationNodeType is NavigationNodeType.PageCollection)
                    {
                        var pageCollectionNavigationData = (PageCollectionNavigationData)pageNode.NavigationData;
                        var title = ((PageCollectionData)item.PageData).Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    else if (pageNode.NavigationNodeType is NavigationNodeType.Page)
                    {
                        var pageCollectionNavigationData = (PlainPageNavigationData)pageNode.NavigationData;
                        var title = ((PlainPageData)item.PageData).Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    //<<
                    //Hieu rem
                    //pageNode.NavigationData.Title.AddOrUpdate<VariationId, string>(variationId, item.PageData.Title.ToString());
                }

                needToUpdateNavigationData = true;
            }

            if (needToUpdateNavigationData)
            {
                var updateNodeResult = await NavigationApiHttpClient.UpdateNodeDataAsync(pageNode);
                updateNodeResult.EnsureSuccessCode();
            }

            if (MigrationSettings.Value.ImportPagesSettings.MigrateImages)
            {
                await ImagesService.MigrateImagesAsync(pageCheckedOutVersion, ImportPagesReport.Instance, migrationItem);
                //return ApiUtils.CreateErrorResponse<INavigationNode>(ex);
            }

            var publishResult = await PageApiHttpClient.PublishAsync(pageCheckedOutVersion);
            publishResult.EnsureSuccessCode();

            await PagesService.UpdatePageSystemInfoAsync(pageId: pageId, versionId: publishResult.Data.Id, page: migrationItem);

            return pageNode;
        }

        //check for 6.0
        private async ValueTask<PageNavigationNode<PageNavigationData>> UpdatePageAsync(PageNavigationMigrationItem migrationItem, PageNavigationNode<PageNavigationData> existingPage)
        {
            //TODO Check 2 new flag ImportBlock & Import Content to return value
            if (!(MigrationSettings.Value.ImportPagesSettings.ImportBlockSettings || MigrationSettings.Value.ImportPagesSettings.ImportPageContent))
                return existingPage;

            var checkoutResult = await PageApiHttpClient.CheckOutAsync(existingPage.Page.Id);
            checkoutResult.EnsureSuccessCode();

            var pageNode = existingPage;
            var pageCheckedOutVersion = checkoutResult.Data;

            PagesService.MergePageData(src: pageCheckedOutVersion.PageData, update: migrationItem.PageData);

            //Note: The boolean for showing in navigation is reverse in G1 and G2, so we need to check for equal value for differences
            bool needToUpdateNavigationData = false;
            if (pageNode.NavigationData.HideInMegaMenu == migrationItem.ShowInMegaMenu ||
                pageNode.NavigationData.HideInCurrentNavigation == migrationItem.ShowInCurrentNavigation)
            {
                pageNode.NavigationData.HideInMegaMenu = !migrationItem.ShowInMegaMenu;
                pageNode.NavigationData.HideInCurrentNavigation = !migrationItem.ShowInCurrentNavigation;
            }

            if (migrationItem.TranslationPages.Count > 0 && MigrationSettings.Value.ImportPagesSettings.ImportTranslationPages == true)
            {
                foreach (var item in migrationItem.TranslationPages)
                {
                    var variationId = (VariationId)MigrationSettings.Value.WCMContextSettings.VariationMappings[item.PageLanguage];

                    //Hieu added
                    if (pageNode.NavigationNodeType is NavigationNodeType.PageCollection)
                    {
                        var pageCollectionNavigationData = (PageCollectionNavigationData)pageNode.NavigationData;
                        var title = ((PageCollectionData)item.PageData).Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    else if (pageNode.NavigationNodeType is NavigationNodeType.Page)
                    {
                        var pageCollectionNavigationData = (PlainPageNavigationData)pageNode.NavigationData;
                        var title = ((PlainPageData)item.PageData).Title;
                        if (!pageCollectionNavigationData.Title.ContainsKey(variationId) || !title.Equals(pageCollectionNavigationData.Title[variationId]))
                        {
                            pageCollectionNavigationData.Title[variationId] = title;
                        }
                    }
                    //<<
                    //Hieu rem
                    //pageNode.NavigationData.Title.AddOrUpdate<VariationId, string>(variationId, item.PageData.Title.ToString());
                }

                needToUpdateNavigationData = true;
            }

            if (needToUpdateNavigationData)
            {
                var updateNodeResult = await NavigationApiHttpClient.UpdateNodeDataAsync(pageNode);
                updateNodeResult.EnsureSuccessCode();
            }

            if (MigrationSettings.Value.ImportPagesSettings.MigrateImages)
            {
                await ImagesService.MigrateImagesAsync(pageCheckedOutVersion, ImportPagesReport.Instance, migrationItem);
            }

            var publishResult = await PageApiHttpClient.PublishAsync(pageCheckedOutVersion);
            publishResult.EnsureSuccessCode();

            await PagesService.UpdatePageSystemInfoAsync(pageId: existingPage.Page.Id, versionId: publishResult.Data.Id, page: migrationItem);

            return existingPage;
        }

        private CreateNavigationRequest CreateNavigationCreationRequest(LinkNavigationMigrationItem link, INavigationNode parentNode)
        {
            //var language = MigrationSettings.Value.WCMContextSettings.Language;
            //Hieu rem
            //var nodeData = new NavigationData
            //{
            //    Title = new VariationString(), 
            //    Type = 8,
            //    RendererId = new Guid("2b416031-3750-4328-ae8e-41a0508939b1"),
            //    AdditionalProperties = new Dictionary<string, JToken>()
            //};

            var nodeData = new LinkNavigationData
            {
                Title = new Fx.Models.Language.MultilingualString(),// VariationString(),
                Type = 8,
                RendererId = new Guid("2b416031-3750-4328-ae8e-41a0508939b1"),
                AdditionalProperties = new Dictionary<string, JToken>()
            };
            //hieu rem
            //nodeData.Title.Add(WcmData.DefaultVariation != null ? (int)WcmData.DefaultVariation.Id : 0, link.Title);
            nodeData.Title.Add(WcmData.DefaultVariation.SupportedLanguages[0].Name, link.Title);
            nodeData.AdditionalProperties.Add("url", link.Url);
            nodeData.AdditionalProperties.Add("openInNewWindow", false);
            nodeData.AdditionalProperties.Add("hideInCurrentNavigation", false);
            nodeData.AdditionalProperties.Add("hideInMegaMenu", false);

            return new CreateNavigationRequest
            {
                NodeData = nodeData,
                Position = new NavigationPosition
                {
                    Parent = parentNode,
                },
            };
        }

        private PageWithNavigationCreationRequest CreatePageWithNavigationCreationRequest(PageNavigationMigrationItem page, INavigationNode parentNode)
        {
            page.PageData.PageRendererId = new Guid("8e012b42-4c13-4150-a11f-6b0b6300ee7c");

            return new PageWithNavigationCreationRequest
            {
                PageData = page.PageData,
                UrlSegment = page.UrlSegment,
                //Hieu rem
                //ChildStructureType = WebContentManagement.Models.OmniaWCMEnums.ChildStructureType.Hierarchical,
                Position = new NavigationPosition
                {

                    Parent = parentNode
                }
            };
        }

        private PageCreationRequest CreatePageCreationRequest(PageNavigationMigrationItem page)
        {
            //Hardcoded to plain page rendererId. TODO: Find constant in WCM
            page.PageData.PageRendererId = new Guid("8e012b42-4c13-4150-a11f-6b0b6300ee7c");

            return new PageCreationRequest
            {
                PageData = page.PageData,
                //SecurityResourceId = WcmData.SecurityResourceId
            };
        }

        private VariationPageCreationRequest createPageVariationCreationRequest(PageNavigationMigrationItem page, PageId originalPage, Variation variation)
        {
            page.PageData.PageRendererId = new Guid("8e012b42-4c13-4150-a11f-6b0b6300ee7c");

            return new VariationPageCreationRequest
            {
                PageData = page.PageData,
                OriginalPageId = originalPage,
                VariationId = variation.Id
            };
        }

        private async Task ImportTranslationPagesAsync(Page originalPage, List<PageNavigationMigrationItem> translationPages)
        {
            if (translationPages == null || translationPages.Count == 0 || MigrationSettings.Value.ImportPagesSettings.ImportTranslationPages == false)
                return;

            var existingVariationPagesResult = await PageApiHttpClient.GetVariations(originalPage);
            existingVariationPagesResult.EnsureSuccessCode();
            var existingVariationPages = existingVariationPagesResult.Data;

            foreach (var translationPage in translationPages)
            {
                translationPage.PageData.IsNullThrow(new Exception("PageData cannot be null"));
                if (!MigrationSettings.Value.WCMContextSettings.VariationMappings.ContainsKey(translationPage.PageLanguage))
                    continue;

                var variationId = MigrationSettings.Value.WCMContextSettings.VariationMappings[translationPage.PageLanguage];
                Variation variation = WcmData.Variations.FirstOrDefault(x => x.Id == variationId);
                variation.IsNullThrow("There is no variation with Id " + variationId);
                //Hieu rem
                //PageDataMapper.MapPageData(translationPage, WcmData.PageTypes, MigrationSettings.Value);
                PageDataMapper.MapPageData(translationPage, WcmData.PageTypes, MigrationSettings.Value, Identities);

                if (existingVariationPages.Any(variationPage => variationPage.VariationId == variationId))
                {
                    var existingVariationPage = existingVariationPages.FirstOrDefault(variationPage => variationPage.VariationId == variationId);

                    var checkoutResult = await PageApiHttpClient.CheckOutAsync(existingVariationPage.Id);
                    checkoutResult.EnsureSuccessCode();

                    var pageCheckedOutVersion = checkoutResult.Data;

                    PagesService.MergePageData(src: pageCheckedOutVersion.PageData, update: translationPage.PageData);

                    if (MigrationSettings.Value.ImportPagesSettings.MigrateImages)
                    {
                        await ImagesService.MigrateImagesAsync(pageCheckedOutVersion, ImportPagesReport.Instance, translationPage);
                    }

                    var publishResult = await PageApiHttpClient.PublishAsync(pageCheckedOutVersion);
                    publishResult.EnsureSuccessCode();

                    await PagesService.UpdatePageSystemInfoAsync(pageId: publishResult.Data.PageId, versionId: publishResult.Data.Id, page: translationPage);
                    await SocialService.ImportCommentsAndLikesAsync(pageId: publishResult.Data.PageId, migrationItem: translationPage, existingPage: null, Identities);
                }
                else
                {

                    var pageCreationRequest = createPageVariationCreationRequest(translationPage, originalPage.Id, variation);

                    var variationCreationResult = await PageApiHttpClient.CreateVariationPage(pageCreationRequest);
                    /*
                    pageCreationResult.EnsureSuccessCode();

                    var variationCreationResult = await PageApiHttpClient.AddVariation(new AddVariationRequest
                    {
                        OriginalPage = originalPage,
                        VariationPage = pageCreationResult.Data.Page,
                        Variation = variation
                    });
                    */
                    variationCreationResult.EnsureSuccessCode();
                    if (MigrationSettings.Value.ImportPagesSettings.MigrateImages)
                    {
                        await ImagesService.MigrateImagesAsync(variationCreationResult.Data.CheckedOutVersion, ImportPagesReport.Instance, translationPage);
                    }
                    var publishResult = await PageApiHttpClient.PublishAsync(variationCreationResult.Data.CheckedOutVersion);
                    publishResult.EnsureSuccessCode();

                    await PagesService.UpdatePageSystemInfoAsync(pageId: publishResult.Data.PageId, versionId: publishResult.Data.Id, page: translationPage);
                    await SocialService.ImportCommentsAndLikesAsync(pageId: publishResult.Data.PageId, migrationItem: translationPage, existingPage: null, Identities);
                }
            }
        }
    }
}

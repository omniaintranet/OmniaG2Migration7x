using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Language;
using Omnia.Fx.Models.Queries;
using Omnia.Fx.Models.Shared;
using Omnia.Fx.Utilities;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.EnterpriseProperties;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Shared;
using Omnia.WebContentManagement.Models.Blocks;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Navigation.HttpContractModels;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Pages.HttpContractModels;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Omnia.Migration.Core.Constants;

namespace Omnia.Migration.Actions
{
    public class ImportPagesAction : ParallelizableMigrationAction
    {
        private PageApiHttpClient PageApiHttpClient { get; }
        private EventApiHttpClient EventApiHttpClient { get; }
        private NavigationApiHttpClient NavigationApiHttpClient { get; }
        private VariationApiHttpClient VariationApiHttpClient { get; }
        private SocialService SocialService { get; }
        private ImagesService ImagesService { get; }
        private PagesService PagesService { get; }
        private PublishingChannelService PublishingChannelService { get; }
        private UserService UserService { get; }
        private WcmService WcmService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private IProgressManager ProgressManager { get; set; }
        private WcmBaseData WcmData { get; set; }
        private Dictionary<Guid, string> PageIdMapping { get; set; }
        private IdentityApiHttpClient IdentityApiHttpClient { get; }
        public ItemQueryResult<IResolvedIdentity> Identities { get; set; }
        public IList<IResolvedIdentity> da { get; set; }
        public List<PublishingChannel> Channels { get; set; }

        public LanguageTag defaultLang = LanguageTag.EnUs;

        Identity currentUser = null;

        private string fileName = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "")+ "Channels.txt";


        public ImportPagesAction(
            PageApiHttpClient pageApiHttpClient,
            EventApiHttpClient eventApiHttpClient,
            NavigationApiHttpClient navigationApiHttpClient,
            VariationApiHttpClient variationApiHttpClient,
            SocialService socialService,
            ImagesService imagesService,
            IdentityApiHttpClient identityApiHttpClient,
            PagesService pagesService,
            PublishingChannelService publishingChannelService,
            UserService userService,
            WcmService wcmService,
            IOptionsSnapshot<MigrationSettings> migrationSettings)

        {
            PageApiHttpClient = pageApiHttpClient;
            EventApiHttpClient = eventApiHttpClient;
            NavigationApiHttpClient = navigationApiHttpClient;

            VariationApiHttpClient = variationApiHttpClient;
            ImagesService = imagesService;
            SocialService = socialService;
            PagesService = pagesService;
            PublishingChannelService = publishingChannelService;
            WcmService = wcmService;
            MigrationSettings = migrationSettings;
            PageIdMapping = new Dictionary<Guid, string>();
            IdentityApiHttpClient = identityApiHttpClient;
            UserService = userService;
        }
        private string LoadSaved(string path)
        {
            string line = "";

            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(path);
                //Read the first line of text
                line = sr.ReadLine();
                string tmp = "";
                //Continue to read until you reach end of file
                while (tmp != null)
                {
                    //Read the next line
                    tmp = sr.ReadLine();
                    line += sr.ReadLine();
                }
                //close the file
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            return line;
        }
        private void UpdateSaved(string jsonObject, string path, bool append = false)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter sw = new StreamWriter(path, append);

                sw.WriteLine(jsonObject);
                //Close the file
                sw.Close();
            }
            catch (Exception e)
            {
                throw e;
                //Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private void LoadSavedChannels(List<PublishingChannel> loadingChannels)
        {
            try
            {
                string data = LoadSaved(fileName);
                List<PublishingChannel> channels = new List<PublishingChannel>();
                channels = JsonConvert.DeserializeObject<List<PublishingChannel>>(data);
                if(channels!= null)
                foreach (var channel in loadingChannels)
                {
                    var savedItem = channels.Where(x=>x.Uid==channel.Uid).FirstOrDefault();
                    if(savedItem != null)
                    {
                        channel.Id = savedItem.Id;
                    }
                }
            }
            catch { }
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
            //  this.Identities = await UserService.LoadUserIdentityTEST();
            this.Identities = await UserService.LoadUserIdentity();

            currentUser = await IdentityApiHttpClient.TryGetCurrentUserIdentityAsync();

            ProgressManager.Start(input.GetTotalCount());
            ImportPagesReport.Instance.Init(MigrationSettings.Value);

            try
            {
                WcmData = await WcmService.LoadWcmBaseDataAsync();

                WcmService.EnsureAndValidateWcmSettings(WcmData);

                if (WcmData.DefaultVariation != null)
                {
                    defaultLang = (LanguageTag)Enum.Parse(typeof(LanguageTag), WcmData.DefaultVariation.SupportedLanguages[0].Name.ToString(), true);
                }

                ImportPublishingChannelObject publishingChannelObj = GetInputPublishingChannels();
                if (publishingChannelObj != null)
                {
                    await PublishingChannelService.EnsureChannelCategoriesAsync(publishingChannelObj.ChannelCategories, defaultLang);
                    LoadSavedChannels(publishingChannelObj.Channels);
                    await PublishingChannelService.EnsureChannelsAsync(publishingChannelObj.Channels, defaultLang, this.Identities, currentUser);
                    //save channels 
                    string jsonObject = JsonConvert.SerializeObject(publishingChannelObj.Channels);
                     
                    UpdateSaved(jsonObject, fileName);

                    this.Channels = publishingChannelObj.Channels;
                }

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
        private ImportPublishingChannelObject GetInputPublishingChannels()
        {
            try
            {
                var inputPublishingChannelsPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportPagesSettings.InputPublishingChannelsFile);
                var importObj = JsonConvert.DeserializeObject<ImportPublishingChannelObject>(File.ReadAllText(inputPublishingChannelsPath));

                return importObj;
            }
            catch { return null; }
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
                    case NavigationMigrationItemTypes.Event:
                        var pageNode = CloneHelper.CloneToPageMigration(node);
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
            EventDetail eventDetail = null;

            try
            {
                migrationItem.PageData.IsNullThrow(new Exception("PageData cannot be null"));

                string eventLocation = string.Empty;
                if (migrationItem.MigrationItemType == NavigationMigrationItemTypes.Event)
                {
                    if (MigrationSettings.Value.WCMContextSettings.EnterprisePropertiesMappings.ContainsKey(BuiltInEnterpriseProperties.EventLocation)
                        && migrationItem.PageData.EnterpriseProperties.ContainsKey(BuiltInEnterpriseProperties.EventLocation))
                    {
                        var terms = migrationItem.PageData.EnterpriseProperties[BuiltInEnterpriseProperties.EventLocation].ToObject<List<G1TaxonomyPropertyValue>>();
                        eventLocation = String.Join(", ", terms.Select(t => t.Label));
                    }
                }

                PageDataMapper.MapPageData(migrationItem, WcmData.PageTypes, MigrationSettings.Value, Identities);

                if (migrationItem.MigrationItemType == NavigationMigrationItemTypes.Event)
                {
                    eventDetail = new()
                    {
                        PageCollectionId = MigrationSettings.Value.WCMContextSettings.PageCollectionId,
                        Title = ((PlainPageData)migrationItem.PageData).Title,
                        StartDate = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventStartDate, out JToken eventStartDate) ? eventStartDate.ToString() : null,
                        EndDate = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventEndDate, out JToken eventEndDate) ? eventEndDate.ToString() : null,
                        MaxParticipants = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventMaxParticipants, out JToken value) ? (int)value : Int32.MaxValue,
                        RegistrationStartDate = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventRegistrationStartDate, out JToken eventRegistrationStartDate) ? eventRegistrationStartDate.ToString() : null,
                        RegistrationEndDate = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventRegistrationEndDate, out JToken eventRegistrationEndDate) ? eventRegistrationEndDate.ToString() : null,
                        CancellationEndDate = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventCancellationEndDate, out JToken eventCancellationEndDate) ? eventCancellationEndDate.ToString() : null,
                        IsOnlineMeeting = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventIsOnlineMeeting, out JToken eventIsOnlineMeeting) && (bool)eventIsOnlineMeeting,
                        ReservationOnly = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventIsReservationOnly, out JToken eventIsReservationOnly) && (bool)eventIsReservationOnly,
                        IsColleague = migrationItem.PageData.EnterpriseProperties.TryGetValue(BuiltInEnterpriseProperties.EventIsSignUpColleague, out JToken eventIsSignUpColleague) && (bool)eventIsSignUpColleague,
                        OutlookEventId = migrationItem.OutlookEventId,
                        Location = eventLocation
                    };
                }

                var existingPage = NavigationNodeHelper.FindMatchingNavigationNode(migrationItem, WcmData.ExistingNodes, parentNode);
                if (existingPage != null)
                {
                    pageNode = await UpdatePageAsync(migrationItem, existingPage);
                }
                else
                {
                    pageNode = await AddNewPageAsync(migrationItem, parentNode, eventDetail);
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

        private async ValueTask<PageNavigationNode<PageNavigationData>> AddNewPageAsync(PageNavigationMigrationItem migrationItem, INavigationNode parentNode, EventDetail eventDetail = null)
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

            if (eventDetail != null)
            {
                await EnsurePageEventAsync(pageId, eventDetail, migrationItem.EventParticipants);
            }

            if (migrationItem.PageChannels.Any())
            {
                await PublishingChannelService.PublishPageToChannelsAsync(pageId, migrationItem.PageChannels, this.Channels);
            }

            await PagesService.UpdatePageSystemInfoAsync(pageId: pageId, versionId: publishResult.Data.Id, page: migrationItem, Identities);

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

            await PagesService.UpdatePageSystemInfoAsync(pageId: existingPage.Page.Id, versionId: publishResult.Data.Id, page: migrationItem, Identities);

            return existingPage;
        }

        private CreateNavigationRequest CreateNavigationCreationRequest(LinkNavigationMigrationItem link, INavigationNode parentNode)
        {

            var nodeData = new LinkNavigationData
            {
                Title = new Fx.Models.Language.MultilingualString(),// VariationString(),
                Type = 8,
                RendererId = new Guid("2b416031-3750-4328-ae8e-41a0508939b1"),
                AdditionalProperties = new Dictionary<string, JToken>(),
                HideInCurrentNavigation = false,
                HideInMegaMenu = false
            };

            nodeData.Title.Add(defaultLang, link.Title);

            var icon = new IconPickerModel { IconType = null, IconSource = "IAutomaticIcon" };

            //hieu rem
            //nodeData.Title.Add(WcmData.DefaultVariation != null ? (int)WcmData.DefaultVariation.Id : 0, link.Title);
            // nodeData.Title.Add(WcmData.DefaultVariation.SupportedLanguages[0].Name, link.Title);
            nodeData.AdditionalProperties.Add("url", link.Url);
            nodeData.AdditionalProperties.Add("openInNewWindow", false);
            nodeData.AdditionalProperties.Add("urlSegment", "");
            nodeData.AdditionalProperties.Add("icon", JToken.FromObject(icon));


            //nodeData.AdditionalProperties.Add("hideInCurrentNavigation", false);
            //  nodeData.AdditionalProperties.Add("hideInMegaMenu", false);

            return new CreateNavigationRequest
            {
                NodeData = nodeData,
                Position = new NavigationPosition
                {
                    Parent = parentNode,
                    After = null,
                    Before = null
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

                    await PagesService.UpdatePageSystemInfoAsync(pageId: publishResult.Data.PageId, versionId: publishResult.Data.Id, page: translationPage, Identities);
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

                    await PagesService.UpdatePageSystemInfoAsync(pageId: publishResult.Data.PageId, versionId: publishResult.Data.Id, page: translationPage, Identities);
                    await SocialService.ImportCommentsAndLikesAsync(pageId: publishResult.Data.PageId, migrationItem: translationPage, existingPage: null, Identities);
                }
            }
        }

        private async Task EnsurePageEventAsync(PageId pageId, EventDetail eventDetail, List<EventParticipant> eventParticipants)
        {
            eventDetail.PageId = pageId;
            var inputOutlookEventId = eventDetail.OutlookEventId;

            var ensureEventResult = await EventApiHttpClient.EnsureEventAsync(eventDetail);
            ensureEventResult.EnsureSuccessCode();

            if (eventParticipants.Any())
            {
                foreach (var participant in eventParticipants)
                {
                    await PagesService.AddEventParticipantAsync(ensureEventResult.Data.Id, participant, Identities);
                }
                await PagesService.UpdateEventDetailsAsync(ensureEventResult.Data.Id, eventParticipants.Where(x => x.ParticipantType == ParticipantType.Official).Sum(x => x.Capacity), inputOutlookEventId);
            }
        }
    }
}

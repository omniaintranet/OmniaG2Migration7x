using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Shared;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Shared;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;

namespace Omnia.Migration.Actions
{

    public class QueryPageAction : BaseMigrationAction
    {
        public class DocumentOutput
        {
            public string pageUrl { get; set; }
            public List<string> docLinks { get; set; }
        }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<GeneratePagesSummaryAction> Logger { get; }
        private WcmBaseData WcmData { get; set; }
        private WcmService WcmService { get; }
        private PageApiHttpClient PageApiHttpClient { get; }
        private List<string> PageList { get; set; }
        private List<string> LayoutIds { get; set; }
        private List<string> ImageURLs { get; set; }
        private List<string> BlockList { get; set; }
        private List<DocumentOutput> DocumentLists { get; set; }
        private List<string> RelatedLinkList { get; set; }

        private string[] keyWords = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".ods", ".txt" };

        public QueryPageAction(
            PageApiHttpClient pageApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            WcmService wcmService,
            ILogger<GeneratePagesSummaryAction> logger)
        {
            MigrationSettings = migrationSettings;
            Logger = logger;
            WcmService = wcmService;
            PageApiHttpClient = pageApiHttpClient;
        }        

        public override async Task StartAsync(IProgressManager progressManager)
        {
            Console.WriteLine("Quey pages.....");
            Console.WriteLine("    1. Query page by GuidLayoutId");
            Console.WriteLine("    2. Query page by key Words");
            string flag = Console.ReadLine();
            List<NavigationMigrationItem> input = ReadInput();
            progressManager.Start(input.GetTotalCount());
            PageList = new List<string>();
            BlockList = new List<string>();
            RelatedLinkList = new List<string>();
            LayoutIds = new List<string>();
            ImageURLs = new List<string>();
            DocumentLists = new List<DocumentOutput>();

            foreach (var item in input)
            { 
                CheckPage(item, string.Empty, progressManager, flag);  
            }

            if(flag =="1")
            {
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPagesResult.json"), JsonConvert.SerializeObject(PageList, Formatting.Indented));
            } 
            else
            {
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_DocumentInPageContent_fixed.json"), JsonConvert.SerializeObject(PageList, Formatting.Indented));
                
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_SVGImage_URL.json"), JsonConvert.SerializeObject(ImageURLs, Formatting.Indented));
                
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_SVGImage_LayoutIds.json"), JsonConvert.SerializeObject(LayoutIds, Formatting.Indented));
                
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_DocumentInBlock.json"), JsonConvert.SerializeObject(BlockList, Formatting.Indented));
                
                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_DocumentInRelatedLinks.json"), JsonConvert.SerializeObject(RelatedLinkList, Formatting.Indented));

                File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueryPages_DocumentLinks_fix.json"), JsonConvert.SerializeObject(DocumentLists, Formatting.Indented));
            }                
        }

        private void CheckPage(NavigationMigrationItem navigationMigrationItem, string parentPageUrl, IProgressManager progressManager, string flag)
        {
            string pageUrl = string.Empty;
                  
            if (navigationMigrationItem.MigrationItemType == NavigationMigrationItemTypes.Page)
            {
                var pageNode = CloneHelper.CloneToPageMigration(navigationMigrationItem);                
                pageUrl = $"{parentPageUrl}/{pageNode.UrlSegment}";
                //if(pageNode.UrlSegment == "policyer")
                //{
                //}
                switch(flag)
                {
                    case "1":
                        if (QueryDocAspxPage(pageNode))
                        {
                            PageList.Add(pageUrl);
                        }
                        break;
                    case "2":
                        FindMatchKeyWords(pageNode, keyWords, pageUrl);
                        break;                    
                    default:
                        break;
                }
            }
            else if (navigationMigrationItem.MigrationItemType == NavigationMigrationItemTypes.Link)
            {
                var linkNode = CloneHelper.CloneToLinkMigration(navigationMigrationItem);
                //pageUrl = $"{parentPageUrl}/{linkNode.Title?.ToLower().Replace(" ", "-")}";
                if (flag == "2")
                {
                    var links = JsonConvert.SerializeObject(linkNode);
                    var docLink = HtmlParser.ParseAllDocumentUrls(links);
                    if (docLink.Count != 0)
                    {
                        var documentOutput = new DocumentOutput();
                        documentOutput.pageUrl = pageUrl;
                        documentOutput.docLinks = docLink;
                        PageList.Add(pageUrl);
                        DocumentLists.Add(documentOutput);
                    }
                }                              
            }

            progressManager.ReportProgress(1);

            foreach (var item in navigationMigrationItem.Children)
            {
                CheckPage(item, pageUrl, progressManager,flag);
            }
        }
        private void FindMatchKeyWords(PageNavigationMigrationItem pageNode, string[] keyword, string pageUrl)
        {
            var enterpriseProperties = pageNode.PageData.EnterpriseProperties;
            if (enterpriseProperties != null)
            {
                var propKeys = enterpriseProperties.Keys.ToList();
                foreach (var propKey in propKeys)
                {
                    if (enterpriseProperties[propKey] == null)
                        continue;
                    var propValueStr = enterpriseProperties[propKey].ToString();
                    var docLink = HtmlParser.ParseAllDocumentUrls(propValueStr);
                    if (docLink.Count != 0)
                    {
                        var documentOutput = new DocumentOutput();
                        documentOutput.pageUrl = pageUrl;
                        documentOutput.docLinks = docLink;
                        PageList.Add(pageUrl);
                        DocumentLists.Add(documentOutput);
                    }
                }
            }

            //
            if (pageNode.BlockSettings != null)
            {
                var blockSettingsJson = JsonConvert.SerializeObject(pageNode.BlockSettings);
                foreach(var block in pageNode.BlockSettings)
                {
                    var oldValue = JToken.FromObject(block.AdditionalProperties["Settings"]);
                    var textValue = oldValue.ToString();
                    var urls = HtmlParser.ParseAllImageUrls(textValue);
                    if (urls.Count == 0) continue;
                    string tempUrl = urls[0].Split(Path.GetFileName(urls[0])).First();
                    ImageURLs.Add(tempUrl);
                }
                var docLink = HtmlParser.ParseAllDocumentUrls(blockSettingsJson);
                if (docLink.Count != 0)
                {
                    var documentOutput = new DocumentOutput();
                    documentOutput.pageUrl = pageUrl;
                    documentOutput.docLinks = docLink;
                    BlockList.Add(pageUrl);
                    DocumentLists.Add(documentOutput);
                }
            }
            //                    
            if (pageNode.RelatedLinks != null)
            {
                var links = JsonConvert.SerializeObject(pageNode.RelatedLinks);
                var docLink = HtmlParser.ParseAllDocumentUrls(links);
                if (docLink.Count != 0)
                {
                    var documentOutput = new DocumentOutput();
                    documentOutput.pageUrl = pageUrl;
                    documentOutput.docLinks = docLink;
                    RelatedLinkList.Add(pageUrl);
                    DocumentLists.Add(documentOutput);
                }
            }
            if (pageNode.PageData.PropertyBag != null)
            {
                var blockdata = JsonConvert.SerializeObject(pageNode.PageData.PropertyBag);
                var docLink = HtmlParser.ParseAllDocumentUrls(blockdata);
                if (docLink.Count != 0)
                {
                    var documentOutput = new DocumentOutput();
                    documentOutput.pageUrl = pageUrl;
                    documentOutput.docLinks = docLink;
                    PageList.Add(pageUrl);
                    DocumentLists.Add(documentOutput);
                }
            }
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

        #region Queries

        public bool QueryByPublishingContact(PageNavigationMigrationItem page)
        {
            try
            {
                var email = "";
                return (page.PageData.EnterpriseProperties != null && 
                        page.PageData.EnterpriseProperties.ContainsKey("PublishingContact") && 
                        page.PageData.EnterpriseProperties["PublishingContact"].ToObject<List<string>>().Any(x => x == email)) ||
                    page.CreatedBy == email ||
                    page.ModifiedBy == email;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public bool QueryDocAspxPage(PageNavigationMigrationItem page)
        {
            //return JsonConvert.SerializeObject(page).ToLower().Contains("doc.aspx");
            //return page.GlueLayoutId == new Guid("777e18dd-c6f4-4458-9838-4ab040d4bb76");
            return page.MigrationItemType == NavigationMigrationItemTypes.Page;
        }        
        #endregion
    }
}

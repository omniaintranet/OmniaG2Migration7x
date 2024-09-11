using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Core.Reports;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnia.Migration.Core;
using Newtonsoft.Json;
using Omnia.Migration.Models.Input.BlockData;
using System.Linq;
using System.IO;
using Omnia.Migration.Core.Extensions;

namespace Omnia.Migration.Actions
{
    public class GeneratePagesSummaryAction : BaseMigrationAction
    {
        private G1SearchPropertiesHttpClient G1SearchPropertiesHttpClient { get; }
        private G1ODMSearchPropertiesHttpClient G1ODMSearchPropertiesHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<GeneratePagesSummaryAction> Logger { get; }
        private List<G1SearchProperty> G1SearchProperties { get; set; }

        public GeneratePagesSummaryAction(
            G1SearchPropertiesHttpClient g1SearchPropertiesHttpClient,
            G1ODMSearchPropertiesHttpClient g1ODMSearchPropertiesHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            ILogger<GeneratePagesSummaryAction> logger)
        {
            G1SearchPropertiesHttpClient = g1SearchPropertiesHttpClient;
            G1ODMSearchPropertiesHttpClient = g1ODMSearchPropertiesHttpClient;
            MigrationSettings = migrationSettings;
            Logger = logger;
        }

        public override async Task StartAsync(IProgressManager progressManager)
        {
            List<NavigationMigrationItem> input = ReadInput();

            progressManager.Start(input.GetTotalCount());
            PagesSummaryReport.Instance.Init(MigrationSettings.Value);

            G1SearchProperties = await GetG1SearchPropertiesAsync();

            foreach (var item in input)
            {
                CheckPage(item, string.Empty, progressManager);
            }

            PagesSummaryReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
        }

        private async ValueTask<List<G1SearchProperty>> GetG1SearchPropertiesAsync()
        {
            List<G1SearchProperty> result = new List<G1SearchProperty>();
            var getSearchPropertyResult = await G1SearchPropertiesHttpClient.GetSearchPropertiesAsync(0);
            result = getSearchPropertyResult.Data;

            if (!string.IsNullOrEmpty(MigrationSettings.Value.OmniaG1Settings.ODMUrl))
            {
                var getODMSearchPropertyResult = await G1ODMSearchPropertiesHttpClient.GetSearchPropertiesAsync();
                result.AddRange(getODMSearchPropertyResult.Data);
            }

            return result;
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
            return input;
        }

        private void CheckPage(NavigationMigrationItem navigationMigrationItem, string parentPageUrl, IProgressManager progressManager)
        {
            string pageUrl = string.Empty;
            if (navigationMigrationItem.MigrationItemType == NavigationMigrationItemTypes.Page)
            {
                var pageNode = CloneHelper.CloneToPageMigration(navigationMigrationItem);
                pageUrl = $"{parentPageUrl}/{pageNode.UrlSegment}";

                PagesSummaryReport.Instance.AllPages.Add(pageUrl);

                CheckPageBlocks(pageNode, pageUrl);
                CheckPageLayout(pageNode, pageUrl);
                CheckEnterpriseProperties(pageNode);
                CheckSearchProperties(pageNode);
            }
            else if (navigationMigrationItem.MigrationItemType == NavigationMigrationItemTypes.Link)
            {
                var linkNode = CloneHelper.CloneToLinkMigration(navigationMigrationItem);
                pageUrl = $"{parentPageUrl}/{linkNode.Title?.ToLower().Replace(" ", "-")}";

                CheckPagesUnderLink(linkNode, pageUrl);
            }

            progressManager.ReportProgress(1);
            PagesSummaryReport.Instance.AllPages.Add(pageUrl);

            foreach (var item in navigationMigrationItem.Children)
            {
                CheckPage(item, pageUrl, progressManager);
            }
        }

        private void CheckPagesUnderLink(LinkNavigationMigrationItem linkNode, string pageUrl)
        {
            var childPages = linkNode.Children.Where(x => x.MigrationItemType == NavigationMigrationItemTypes.Page).ToList();
            foreach (var child in childPages)
            {
                PagesSummaryReport.Instance.PagesUnderLink.Add(pageUrl);
            }
        }

        private void CheckPageBlocks(PageNavigationMigrationItem page, string pageUrl)
        {
            foreach (var block in page.BlockSettings)
            {
                switch (block.ControlId.ToString().ToUpper())
                {
                    case Constants.G1ControlIDs.BannerIdString:
                        PagesSummaryReport.Instance.PagesWithBanner.Add(pageUrl);
                        break;
                    case Constants.G1ControlIDs.DocumentRollupIdString:
                        PagesSummaryReport.Instance.PagesWithDocumentRollup.Add(pageUrl);
                        break;
                    case Constants.G1ControlIDs.ControlledDocumentViewIdString:
                        PagesSummaryReport.Instance.PagesWithControlledDocumentView.Add(pageUrl);
                        break;
                    case Constants.G1ControlIDs.PeopleRollupIdString:
                        PagesSummaryReport.Instance.PagesWithPeopleRollup.Add(pageUrl);
                        break;
                    default:
                        PagesSummaryReport.Instance.AddPageWithOtherBlocks(pageUrl, block.ControlId);
                        break;
                }               
            }
        }

        private void CheckPageLayout(PageNavigationMigrationItem page, string pageUrl)
        {
            if (page.GlueLayoutId != null && IsCustomLayout(page.GlueLayoutId.Value))
            {
                PagesSummaryReport.Instance.AddCustomPageLayout(page.GlueLayoutId.Value, page.BlockSettings, pageUrl);
            }
        }

        private void CheckEnterpriseProperties(PageNavigationMigrationItem page)
        {
            if (page.PageData != null && page.PageData.EnterpriseProperties != null)
            {
                foreach (var property in page.PageData.EnterpriseProperties.Keys)
                {
                    PagesSummaryReport.Instance.AddEnterpriseProperty(property);
                }
            }
        }

        private void CheckSearchProperties(PageNavigationMigrationItem page)
        {
            if (page.BlockSettings != null)
            {
                var blockSettingsJson = JsonConvert.SerializeObject(page.BlockSettings);
                G1SearchProperties = G1SearchProperties.Where(p => p.displayName != null).ToList();
                foreach (var prop in G1SearchProperties)
                {
                    var propNames = JsonConvert.DeserializeObject<List<G1LocalizedText>>(prop.displayName);
                    var defaultPropName = propNames.FirstOrDefault(x => x.language == MigrationSettings.Value.WCMContextSettings.Language)?.value;

                    if (string.IsNullOrEmpty(defaultPropName))
                        defaultPropName = propNames.FirstOrDefault(x => string.IsNullOrEmpty(x.language))?.value;

                    if (string.IsNullOrEmpty(defaultPropName))
                        defaultPropName = propNames.FirstOrDefault()?.value;

                    if (propNames.Any(x => blockSettingsJson.Contains("{Property." + x.value + "}")))
                    {
                        PagesSummaryReport.Instance.AddSearchProperty(prop, defaultPropName);
                    }

                    if (blockSettingsJson.Contains(prop.id.ToString()) || blockSettingsJson.Contains(prop.id.ToString().ToLower()))
                    {
                        PagesSummaryReport.Instance.AddSearchProperty(prop, defaultPropName);
                    }
                }
            }
        }

        private bool IsCustomLayout(Guid pageLayoutId)
        {
            return pageLayoutId != Constants.G1GlueLayoutIDs.PageWithLeftNav &&
                pageLayoutId != Constants.G1GlueLayoutIDs.PageWithoutLeftNav &&
                pageLayoutId != Constants.G1GlueLayoutIDs.StartPage &&
                pageLayoutId != Constants.G1GlueLayoutIDs.NewsArticle;
        }


    }
}

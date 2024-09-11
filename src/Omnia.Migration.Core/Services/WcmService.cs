using Microsoft.Extensions.Options;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Shared;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
    public class WcmService
    {
        private NavigationApiHttpClient NavigationApiHttpClient { get; }
        private VariationApiHttpClient VariationApiHttpClient { get; }
        private PageApiHttpClient PageApiHttpClient { get; }
        private EnterprisePropertiesApiHttpClient EnterprisePropertiesApiHttpClient { get; }
        private PagesService PagesService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public WcmService(
            NavigationApiHttpClient navigationApiHttpClient,
            VariationApiHttpClient variationApiHttpClient,
            PageApiHttpClient pageApiHttpClient,
            EnterprisePropertiesApiHttpClient enterprisePropertiesApiHttpClient,
            PagesService pagesService,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            NavigationApiHttpClient = navigationApiHttpClient;
            VariationApiHttpClient = variationApiHttpClient;
            PageApiHttpClient = pageApiHttpClient;
            EnterprisePropertiesApiHttpClient = enterprisePropertiesApiHttpClient;
            PagesService = pagesService;
            MigrationSettings = migrationSettings;
        }

        public async ValueTask<WcmBaseData> LoadWcmBaseDataAsync()
        {
            WcmBaseData baseData = new WcmBaseData();

            var enterprisePropsResult = await EnterprisePropertiesApiHttpClient.GetEnterprisePropertiesAsync();
            enterprisePropsResult.EnsureSuccessCode();

            baseData.EnterpriseProperies = enterprisePropsResult.Data;

            baseData.PageCollectionId = MigrationSettings.Value.WCMContextSettings.PageCollectionId;

            var pageCollectionResult = await NavigationApiHttpClient.GetPageNavigationNodesAsync(new PageId[] { baseData.PageCollectionId });

            var pageCollectionNode = await PagesService.GetPageCollectionNodeAsync(baseData.PageCollectionId.Value);
            baseData.SecurityResourceId = pageCollectionNode.Page.SecurityResourceId;
            baseData.PageCollectionNode = pageCollectionNode;

            var getExistingNodesResult = await NavigationApiHttpClient.GetChildrenAsync(pageCollectionNode, -1, -1);
            getExistingNodesResult.EnsureSuccessCode();
            baseData.ExistingNodes = getExistingNodesResult.Data;

            var children = await NavigationApiHttpClient.GetChildrenAsync(pageCollectionNode);
            children.EnsureSuccessCode();

            var getVariationsResult = await VariationApiHttpClient.Get(MigrationSettings.Value.WCMContextSettings.PublishingAppId);
            getVariationsResult.EnsureSuccessCode();
            baseData.Variations = getVariationsResult.Data ?? new List<Variation>();

            baseData.DefaultVariation = baseData.Variations.FirstOrDefault(x => x.IsDefault);
            MigrationSettings.Value.WCMContextSettings.DefaultVariationId = baseData.DefaultVariation != null ? (int)baseData.DefaultVariation.Id : 0;

            baseData.PageTypes = new Dictionary<int, PublishedVersionPageData<PageData>>();
            var pageTypeIds = MigrationSettings.Value.WCMContextSettings.LayoutMappings.Select(x => x.Value.LayoutId).Distinct().ToList();

            foreach (var layoutMapping in MigrationSettings.Value.WCMContextSettings.LayoutMappings)
            {
                var pageTypeId = layoutMapping.Value.LayoutId;
                var getPageTypeResult = await PageApiHttpClient.GetPublishedVersionAsync(pageTypeId);
                getPageTypeResult.EnsureSuccessCode();

                baseData.PageTypes.AddOrUpdate(pageTypeId, getPageTypeResult.Data);
            }

            return baseData;
        }

        public void EnsureAndValidateWcmSettings(WcmBaseData wcmBaseData)
        {
            var wcmSettings = MigrationSettings.Value.WCMContextSettings;

            ValidateBuiltInProperties(wcmBaseData, wcmSettings);

            ValidateEnterprisePropertyMappings(wcmBaseData, wcmSettings);

            ValidateSearchPropertyMappings(wcmBaseData, wcmSettings);

            ValidateVariationMappings(wcmBaseData, wcmSettings);

            if (string.IsNullOrEmpty(wcmSettings.DatabaseConnectionString))
                throw new Exception("WCM database connection string is missing");

            EnsureWcmAutoLayoutMappings(wcmBaseData, wcmSettings);
        }

        private void EnsureWcmAutoLayoutMappings(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            foreach (var layoutMapping in wcmSettings.LayoutMappings)
            {
                var pageTypeId = layoutMapping.Value.LayoutId;
                if (layoutMapping.Value.UseAutoMapping)
                {
                    var layoutData = wcmBaseData.PageTypes[pageTypeId].PageData.Layout;
                    layoutMapping.Value.PageImageBlock = LayoutManager.ExtractBlockIdForPageProperty(layoutData, Core.Constants.BuiltInEnterpriseProperties.PageImage);
                    layoutMapping.Value.MainContentBlock = LayoutManager.ExtractBlockIdForPageProperty(layoutData, Core.Constants.BuiltInEnterpriseProperties.PageContent);
                    layoutMapping.Value.RelatedLinksBlock = LayoutManager.ExtractBlockIdForPageProperty(layoutData, wcmSettings.DefaultRelatedLinksProperty);
                    layoutMapping.Value.AccordionBlock = LayoutManager.ExtractBlockIdForPageProperty(layoutData, wcmSettings.DefaultAccordionProperty);
                   
                    layoutMapping.Value.ZoneMappings = new Dictionary<string, string>();
                    string rightSectionId = LayoutManager.ExtractContainerIdForLayoutItem(layoutData.Definition.Items, layoutMapping.Value.RelatedLinksBlock)?.ToString();
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.RightZone, rightSectionId);
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.Zone5, rightSectionId);
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.Zone2, rightSectionId);
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.Zone1, rightSectionId);
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.Zone3, rightSectionId);

                    string mainSectionId = LayoutManager.ExtractContainerIdForLayoutItem(layoutData.Definition.Items, layoutMapping.Value.MainContentBlock)?.ToString();
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.MainZone, mainSectionId);
                    layoutMapping.Value.ZoneMappings.AddIfNotNull(Core.Constants.G1GlueLayoutZoneIDs.Zone4, mainSectionId);
                }
            }
        }

        private void ValidateBuiltInProperties(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            ValidateDefaultPeopleNameProperty(wcmBaseData, wcmSettings);
            ValidateDefaultRelatedLinksProperty(wcmBaseData, wcmSettings);
            ValidateDefaultAccordionProperty(wcmBaseData, wcmSettings);
            ValidateDefaultSVGViewerProperty(wcmBaseData, wcmSettings);
        }

        private void ValidateDefaultPeopleNameProperty(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            if (string.IsNullOrEmpty(wcmSettings.DefaultPeopleNameProperty))
                wcmSettings.DefaultPeopleNameProperty = Core.Constants.BuiltInEnterpriseProperties.PeoplePreferredName;

            if (!wcmBaseData.EnterpriseProperies.Any(x => x.InternalName == wcmSettings.DefaultPeopleNameProperty))
            {
                throw new Exception("Mapped people name property does not exist in G2: " + wcmSettings.DefaultPeopleNameProperty);
            }
        }

        private void ValidateDefaultRelatedLinksProperty(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            var defaultRelatedLinksProperty = wcmBaseData.EnterpriseProperies.FirstOrDefault(x =>
                    x.InternalName == Core.Constants.BuiltInEnterpriseProperties.RelatedLinks ||
                    x.InternalName == Core.Constants.BuiltInEnterpriseProperties.RelatedLinks2);

            if (string.IsNullOrEmpty(wcmSettings.DefaultRelatedLinksProperty))
            {
                wcmSettings.DefaultRelatedLinksProperty = defaultRelatedLinksProperty?.InternalName;
            }

            if (!wcmBaseData.EnterpriseProperies.Any(x => x.InternalName == wcmSettings.DefaultPeopleNameProperty))
            {
                throw new Exception("Mapped related links property does not exist in G2: " + wcmSettings.DefaultRelatedLinksProperty);
            }
        }

        private void ValidateDefaultAccordionProperty(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            if (!string.IsNullOrEmpty(wcmSettings.DefaultAccordionProperty) && 
                !wcmBaseData.EnterpriseProperies.Any(x => x.InternalName == wcmSettings.DefaultAccordionProperty))
            {
                throw new Exception("Mapped accordion property does not exist in G2: " + wcmSettings.DefaultAccordionProperty);
            }
        }
        private void ValidateDefaultSVGViewerProperty(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            if (!string.IsNullOrEmpty(wcmSettings.DefaultSVGViewerProperty) &&
                !wcmBaseData.EnterpriseProperies.Any(x => x.InternalName == wcmSettings.DefaultSVGViewerProperty))
            {
                throw new Exception("Mapped accordion property does not exist in G2: " + wcmSettings.DefaultSVGViewerProperty);
            }
        }
        private void ValidateEnterprisePropertyMappings(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            var wrongPropMappings = wcmSettings.EnterprisePropertiesMappings
                .Where(x => !wcmBaseData.EnterpriseProperies.Any(prop => x.Value.PropertyName.ToLower() == prop.InternalName.ToLower()))
                .ToList();

            if (wrongPropMappings.Any())
                throw new Exception("Mapped enterprise properties do not exist in G2: " + string.Join(", ", wrongPropMappings.Select(x => x.Value.PropertyName)));
        }

        private void ValidateSearchPropertyMappings(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            var wrongSearchPropMappings = wcmSettings.SearchProperties
                .Where(x => !wcmBaseData.EnterpriseProperies.Any(prop =>
                        x.G2PropertyName.ToLower() == prop.InternalName.ToLower()))
                .ToList();

            if (wrongSearchPropMappings.Any())
                throw new Exception("Mapped search properties does not exist in G2 or managed properties do not match: " + string.Join(", ", wrongSearchPropMappings.Select(x => x.G2PropertyName)));
        }

        private void ValidateVariationMappings(WcmBaseData wcmBaseData, WCMContextSettings wcmSettings)
        {
            foreach (var variationMapping in wcmSettings.VariationMappings)
            {
                var variation = wcmBaseData.Variations.FirstOrDefault(x => x.Id == variationMapping.Value);
                if (variation == null)
                    throw new Exception($"Variation with Id {variationMapping.Value} does not exist in G2");
            }
        }
    }
}

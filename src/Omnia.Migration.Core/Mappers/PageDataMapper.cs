using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Factories;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models;
using Omnia.Migration.Models.BlockData;
using Omnia.Migration.Models.EnterpriseProperties;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Mappings;
using Omnia.WebContentManagement.Models.Layout;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UrlCombineLib;
using Omnia.Migration.Core.Extensions;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;

namespace Omnia.Migration.Core.Mappers
{
    public static class PageDataMapper
    {
        public static void MapPageData(PageNavigationMigrationItem page, Dictionary<int, PublishedVersionPageData<PageData>> pageTypes, MigrationSettings settings, ItemQueryResult<IResolvedIdentity> Identities)
        {
            EnsureDataFormat(page);

            MapSystemProperties(page, Identities);
            MapEnterpriseProperties(page, settings.WCMContextSettings, Identities);
            MapLayout(page, pageTypes, settings.WCMContextSettings);
        }

        private static void MapLayout(PageNavigationMigrationItem page, Dictionary<int, PublishedVersionPageData<PageData>> pageTypes, WCMContextSettings wcmSettings)
        {
            var layoutsMap = wcmSettings.LayoutMappings;

            string glueLayoutId = (page.GlueLayoutId ?? Guid.Empty).ToString().ToLower();
            LayoutMapping layoutMapping = null;

            if (layoutsMap.ContainsKey(glueLayoutId))
            {
                layoutMapping = layoutsMap[glueLayoutId];
            }
            else
            {
                layoutMapping = layoutsMap[Guid.Empty.ToString()];
            }

            page.PageData.ParentLayoutPageId = layoutMapping.LayoutId;

            MapLayoutBlockData(page, layoutMapping, wcmSettings);
            MapCustomBlockData(page, layoutMapping, pageTypes, wcmSettings);
        }

        //Hieu added
        private static string GetSystemPropUserIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return item.Id + "[1]";
                }
            }
            return null;
        }
        //<<
        public static string UpdatePageImage(PageNavigationMigrationItem item)
        {
            var blocks = item.BlockSettings;
            if (blocks != null)
            {
                foreach (var block in blocks)
                {

                    if (block.ControlId.ToString().ToUpper() == "E1EB6412-5B5E-448F-B3A4-FCD3C7867A33")
                    {
                        
                        var result = "<img alt=\"\" src=\"" + block.AdditionalProperties["Settings"]["imageUrl"].ToString() + "\"style=\"BORDER: px solid; \">";
                        return result;
                    }
                }
            }
            return null;
        }
        private static void MapEnterpriseProperties(PageNavigationMigrationItem page, WCMContextSettings wcmSettings, ItemQueryResult<IResolvedIdentity> Identities)
        {
            // Fixed for SWEP Mediablock
            var a = page.PageData.EnterpriseProperties["pageimage"];
            if (a.ToString().Length < 1)
            {
                page.PageData.EnterpriseProperties["pageimage"] = UpdatePageImage(page);
            }
           
            var propertiesMap = wcmSettings.EnterprisePropertiesMappings;

            if (page.PageData.EnterpriseProperties == null || propertiesMap == null)
                return;

            var newProps = new PageEnterprisePropertyDictionary();
            //Hieu rem
            //newProps.Add("title", page.PageData.Title);
            //Set defaut value for custom property for Kungsbacka
            // newProps.AddOrUpdateTaxonomy("kbkPageType", new Guid("72d6c60d-0877-459e-bd4f-a6e619731904"));            
            newProps.Add("title", ((PlainPageData)page.PageData).Title);
            newProps.Add("OmniaObjectType", "[\"ef2f10ff-c790-44f3-9219-3d2577a6fcc8\"]");

            var propsToMap = page.PageData.EnterpriseProperties.Keys.Where(key => propertiesMap.ContainsKey(key)).ToList();


            foreach (var oldProp in propsToMap)
            {
                var propValue = page.PageData.EnterpriseProperties[oldProp];
                var newProp = propertiesMap[oldProp];

                
                switch (newProp.PropertyType)
                {
                    case EnterprisePropertyType.MainContent:
                        page.MainContent = EnterprisePropertyMapper.MapTextPropertyValue(propValue, wcmSettings).ToString();
                        newProps.Add(newProp.PropertyName, page.MainContent);
                        break;
                    case EnterprisePropertyType.Image:
                        var newImagePropValue = EnterprisePropertyMapper.MapImagePropertyValue(propValue, wcmSettings);
                        newProps.Add(newProp.PropertyName, newImagePropValue);
                        break;
                    case EnterprisePropertyType.User:
                        //Hieu rem
                        //var newUserPropValue = EnterprisePropertyMapper.MapUserPropertyValue(propValue);
                        var newUserPropValue = EnterprisePropertyMapper.MapUserPropertyValue(propValue, Identities);
                        newProps.Add(newProp.PropertyName, newUserPropValue);
                        break;
                    case EnterprisePropertyType.Taxonomy:
                        var newTaxonomyPropValue = EnterprisePropertyMapper.MapTaxonomyPropertyValue(propValue);
                        newProps.Add(newProp.PropertyName, newTaxonomyPropValue);
                        break;
                    case EnterprisePropertyType.Boolean:
                        var newBooleanPropValue = EnterprisePropertyMapper.MapBooleanPropertyValue(propValue);
                        newProps.Add(newProp.PropertyName, newBooleanPropValue);
                        break;
                    case EnterprisePropertyType.Datetime:
                        var newDateTimePropertyValue = EnterprisePropertyMapper.MapDateTimePropertyValue(propValue);
                        newProps.Add(newProp.PropertyName, newDateTimePropertyValue);
                        break;
                    case EnterprisePropertyType.Object:
                        if (propValue.HasValues)
                        {
                            var newJsonObjPropertyValue = propValue;
                            newProps.Add(newProp.PropertyName, newJsonObjPropertyValue);
                        }
                        break;
                    default:
                        var newTextPropertyValue = EnterprisePropertyMapper.MapTextPropertyValue(propValue, wcmSettings);
                        newProps.Add(newProp.PropertyName, newTextPropertyValue);
                        break;
                }
            }

            page.PageData.EnterpriseProperties = newProps;
        }

        private static void MapSystemProperties(PageNavigationMigrationItem page, ItemQueryResult<IResolvedIdentity> Identities)
        {
            if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.CreatedAt))
                page.CreatedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedAt].ToString();
            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.CreatedAtUpperCase))
            {
                page.CreatedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedAtUpperCase].ToString();
            }
            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.CreatedAtUpperCase2))
            {
                page.CreatedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedAtUpperCase2].ToString();
            }
            if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.CreatedBy))
                //Hieu rem
                //page.CreatedBy = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedBy].ToString();
                page.CreatedBy = GetSystemPropUserIdentitybyEmail(Identities, page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedBy].ToString());
            // Thoan modify


            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.CreatedByUpperCase))
            {
                //Hieu rem
                //page.CreatedBy = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedByUpperCase].ToString();
                page.CreatedBy = GetSystemPropUserIdentitybyEmail(Identities, page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.CreatedByUpperCase].ToString());
            }
            if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.ModifiedAt))
                page.ModifiedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedAt].ToString();
            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.ModifiedAtUpperCase))
            {
                //Hieu rem
                page.ModifiedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedAtUpperCase].ToString();
            }
            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.ModifiedAtUpperCase2))
            {
                page.ModifiedAt = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedAtUpperCase2].ToString();
            }
            if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.ModifiedBy))
                //Hieu rem
                //page.ModifiedBy = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedBy].ToString();
                page.ModifiedBy = GetSystemPropUserIdentitybyEmail(Identities, page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedBy].ToString());
            else if (page.PageData.EnterpriseProperties.ContainsKey(Constants.BuiltInEnterpriseProperties.ModifiedByUpperCase))
            {
                //Hieu rem
                //page.ModifiedBy = page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedByUpperCase].ToString();
                page.ModifiedBy = GetSystemPropUserIdentitybyEmail(Identities, page.PageData.EnterpriseProperties[Constants.BuiltInEnterpriseProperties.ModifiedByUpperCase].ToString());

            }

        }

        private static void MapLayoutBlockData(PageNavigationMigrationItem page, LayoutMapping layoutMapping, WCMContextSettings wcmSettings)
        {
            /*if (page.PageData.LayoutData.BlockData == null)
                page.PageData.LayoutData.BlockData = new Dictionary<Guid, Omnia.Migration.Models.LegacyWCM.BlockData>();

            if (!layoutMapping.PageImageBlock.IsNullOrEmpty())
            {
                page.PageData.LayoutData.BlockData[layoutMapping.PageImageBlock.Value] = CreateBlockDataForPageImage(page);
            }
            */
            if (!layoutMapping.MainContentBlock.IsNullOrEmpty())
            {
                //page.PageData.LayoutData.BlockData[layoutMapping.MainContentBlock.Value] = CreateBlockDataForMainContent(page);
                //page.PageData.PropertyBag.AddOrUpdateContentBlockProperty = CreateBlockDataForMainContent(page);
                //var relatedLinksBlockSettings = CreateBlockDataForMainContent(wcmSettings.EnterprisePropertiesMappings);
            }

            if (!layoutMapping.RelatedLinksBlock.IsNullOrEmpty())
            {
                var relatedLinksBlockSettings = CreateBlockDataForRelatedLinks(page, wcmSettings);
                //page.PageData.LayoutData.BlockData[layoutMapping.RelatedLinksBlock.Value] = relatedLinksBlockSettings;
                page.PageData.EnterpriseProperties[wcmSettings.DefaultRelatedLinksProperty] = JToken.FromObject(relatedLinksBlockSettings.Data);
            }
            if (!layoutMapping.UseAutoMapping)
            {

                //Thoan - For IKEA Test Migration

                //if(!layoutMapping.ZoneMappings["RelatedLinksBlock"].IsNull())
                //{
                //    var relatedLinksBlockSettings = CreateBlockDataForRelatedLinks(page, wcmSettings);
                //    //page.PageData.LayoutData.BlockData[layoutMapping.RelatedLinksBlock.Value] = relatedLinksBlockSettings;
                //    page.PageData.EnterpriseProperties[wcmSettings.DefaultRelatedLinksProperty] = JToken.FromObject(relatedLinksBlockSettings.Data);
                //}    
            }


            //}
            /*if (!layoutMapping.AccordionBlock.IsNullOrEmpty())
            {
                var accordionBlockSettins = page.BlockSettings.FirstOrDefault(x => x.ControlId == Constants.G1ControlIDs.Accordion);

                if (accordionBlockSettins != null)
                {
                    page.BlockSettings.Remove(accordionBlockSettins);
                    var accordionBlockSettings = BlockDataMapper.MapBlockData(accordionBlockSettins, wcmSettings);
                    page.PageData.LayoutData.BlockData[layoutMapping.AccordionBlock.Value] = accordionBlockSettings;

                    if (!string.IsNullOrEmpty(wcmSettings.DefaultAccordionProperty))
                    {
                        page.PageData.EnterpriseProperties[wcmSettings.DefaultAccordionProperty] = JToken.FromObject(accordionBlockSettings.Data);
                    }
                }
            }*/
        }

        //Check for 6.0
        private static void MapCustomBlockData(PageNavigationMigrationItem page, LayoutMapping layoutMapping, Dictionary<int, PublishedVersionPageData<PageData>> pageTypes, WCMContextSettings wcmSettings)
        {
            if (!pageTypes.ContainsKey(layoutMapping.LayoutId))
            {
                //TODO: Write to report
                return;
            }

            //Hieu rem
            //var parentLayout = pageTypes[layoutMapping.LayoutId].PageData.Layout.Definition;
            var parentLayout = pageTypes[layoutMapping.LayoutId].PageData.Layout;
            //LayoutManager.InjectParentLayout(page.PageData.LayoutData.Layout, parentLayout);
            //Hieu rem
            //page.PageData.Layout.Definition = Omnia.Fx.Models.Layouts.LayoutManager.InjectParentLayout(page.PageData.Layout.Definition, parentLayout.Definition);
            page.PageData.Layout = Omnia.Fx.Models.Layouts.LayoutManager.InjectParentLayout(page.PageData.Layout, parentLayout);
            foreach (var block in page.BlockSettings)
            {
                var srcZoneId = block.ZoneId;
                if (!layoutMapping.ZoneMappings.ContainsKey(srcZoneId))
                {
                    //TODO: Write to report
                    continue;
                }

                var destZoneId = new Guid(layoutMapping.ZoneMappings[srcZoneId]);
                var layoutContainer = LayoutManager.FindLayoutItemRecursive(page.PageData.Layout.Definition, destZoneId);
                if (layoutContainer == null)
                {
                    //TODO: Write to report
                    continue;
                }

                var newBlockData = BlockDataMapper.MapBlockData(block, wcmSettings);

                if (newBlockData != null)
                {
                    var newBlockId = block.InstanceId;

                    //page.PageData.LayoutData.BlockData.Add(newBlockId, newBlockData);
                    //page.PageData.PropertyBag.Add($"blockprop-{newBlockId.ToString().ToLower()}", JToken.FromObject(newBlockData.Settings));
                    page.PageData.Layout.BlockSettings.AddOrUpdate(newBlockId, newBlockData.Settings);
                    page.PageData.PropertyBag.Add($"blockprop-{newBlockId.ToString().ToLower()}", JToken.FromObject(newBlockData.Data));

                    ////Custom for SVG Viewer - Kungsbacka
                    //if(block.ControlId.ToString()== Constants.G1ControlIDs.SVGViewerIdString.ToString().ToLower())
                    //{
                    //    page.PageData.EnterpriseProperties[wcmSettings.DefaultSVGViewerProperty] = JToken.FromObject(newBlockData.Data);
                    //}  
                    if (layoutContainer.Items == null)
                        layoutContainer.Items = new List<Omnia.Fx.Models.Layouts.LayoutItem>();

                    var newLayoutItem = new Omnia.Fx.Models.Layouts.LayoutItem
                    {
                        Id = newBlockId,
                        Itemtype = "block",
                        OwnerLayoutId = page.PageData.Layout.Definition.Id,
                        AdditionalProperties = new Dictionary<string, JToken>()
                    };
                    newLayoutItem.AdditionalProperties["ElementName"] = newBlockData.GetElementName();
                    newLayoutItem.AdditionalProperties["Settings"] = JToken.FromObject(new BlockLayoutItemSettings()
                    {
                        background = new BlockLayoutSettingsBackground
                        {
                            colors = new List<string> { "#fff" },
                            elevation = 0, // set elevation for setting style of block
                            //borderWidth = 1  // Set border of block to 1px 
                        }
                    });

                    layoutContainer.Items.Add(newLayoutItem);

                }
                else
                {
                    //TODO: Write to report
                }
            }
            //Hieu rem
            //Omnia.Fx.Models.Layouts.LayoutManager.RemoveParentLayoutItems(page.PageData.Layout.Definition);
            Omnia.Fx.Models.Layouts.LayoutManager.RemoveParentLayoutItems(page.PageData.Layout);
        }

        private static Omnia.Migration.Models.LegacyWCM.BlockData CreateBlockDataForRelatedLinks(PageNavigationMigrationItem page, WCMContextSettings wcmSettings)
        {
            List<RelatedLink> links = new List<RelatedLink>();
            if (page.RelatedLinks != null)
            {
                links = page.RelatedLinks.Select(x => LinkMapper.MapRelatedLink(x, wcmSettings)).ToList();
            }

            return BlockDataFactory.CreateRelatedLinksBlockData(links);
        }
        private static Omnia.Migration.Models.LegacyWCM.BlockData CreateBlockDataForMainContent(PageNavigationMigrationItem page)
        {
            return BlockDataFactory.CreateContentBlockData(string.Empty, page.MainContent);
        }

        /*private static BlockData CreateBlockDataForPageImage(PageNavigationMigrationItem page)
        {
            return BlockDataFactory.CreateMediaBlockData(string.Empty, true, false);
        }     */

        private static void EnsureDataFormat(PageNavigationMigrationItem migrationItem)
        {
            if (migrationItem.PageData.PropertyBag == null)
                migrationItem.PageData.PropertyBag = new PagePropertyBagDictionary();


            migrationItem.PageData.Layout = new Omnia.Fx.Models.Layouts.Layout();
            migrationItem.PageData.Layout.Definition = LayoutDataFactory.New();
            migrationItem.PageData.Layout.BlockSettings = new Dictionary<Guid, Fx.Models.Layouts.BlockSettings>();

            /*if (migrationItem.PageData.LayoutData == null)
            {
                migrationItem.PageData.LayoutData = new PageLayoutData
                {
                    Layout = LayoutDataFactory.New(),
                    BlockData = null,
                    ParentLayoutPageId = null
                };
            }*/

            if (migrationItem.Comments == null)
                migrationItem.Comments = new List<Models.Input.Social.G1Comment>();
            if (migrationItem.Likes == null)
                migrationItem.Likes = new List<Models.Input.Social.G1Like>();

            if (migrationItem.PageData.EnterpriseProperties == null)
                migrationItem.PageData.EnterpriseProperties = new PageEnterprisePropertyDictionary();
        }
    }
}

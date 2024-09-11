using Omnia.Migration.Core.Factories;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models.BlockData;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.EnterpriseProperties;
using Omnia.Migration.Models.Input.BlockData;
using Omnia.WebContentManagement.Models.Variations;
using Omnia.Fx;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using Omnia.Migration.Core.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.IO;
using static Omnia.WebContentManagement.Models.Blocks.PeopleRollupEnums;
using static Omnia.Migration.Models.Input.BlockData.G1PeopleRollupSettingData;
using Omnia.Foundation.Models.Search;
using Omnia.Fx.Models.EnterpriseProperties;
using static Omnia.WebContentManagement.Models.OmniaWCMEnums;

namespace Omnia.Migration.Core.Mappers
{
    enum PropertyIndexedType
    {
        Text = 1,
        Number = 2,
        DateTime = 3,
        Boolean = 4,
        Person = 5,
        Taxonomy = 6,
        EnterpriseKeywords = 7,
        Media = 8,
        RichText = 9,
        Data = 10,
        Language = 11,
        Tags = 12
    }
    enum DateTimeMode
    {
        Normal = 1,
        Social = 2
    }
    public static class BlockDataMapper
    {
        
        private static string PeopleRollupDefaultPeopleNameField = "panpreferredname";
        private static string PeopleRollupDefaultView = "9c5b9965-05a0-4ea8-905a-352323610f30";

        private static string DocumentRollupDefaultSortBy = "odmfilename";
        private static string DocumentRollupDefaultView = "0573c149-cac2-461e-818a-e6011ca60cc1";
        private static DocumentRollupListViewSettings DocumentRollupDefaultViewSettings = new DocumentRollupListViewSettings
        {
            selectProperties = new List<string> { "title", "FileName" },
            columns = new List<RollupListViewSettingsColumn> {
                    //new RollupListViewSettingsColumn { internalName = "b18ac29d-b579-4971-ac5e-c0f71ad33ae1" }, // FileName column
                     new RollupListViewSettingsColumn { internalName = "cd49fdbc-ea98-4bfd-ad97-f471b68c4505" }, // Title column
                    new RollupListViewSettingsColumn { internalName = "cbc0ab3a-fc36-4ebe-97d2-75cc450ed2be" } // Info icon column
                }
        };

        private static DocumentRollupListViewSettings ControlledDocumentDefaultViewSettings = new DocumentRollupListViewSettings
        {
            selectProperties = new List<string> { "title", "FileName", "c66e052d-6b51-4f66-935b-376f2e5163a9" },
            columns = new List<RollupListViewSettingsColumn> {
                    //new RollupListViewSettingsColumn { internalName = "b18ac29d-b579-4971-ac5e-c0f71ad33ae1" }, // FileName column
                    new RollupListViewSettingsColumn { internalName = "cd49fdbc-ea98-4bfd-ad97-f471b68c4505" }, // Title column
                    new RollupListViewSettingsColumn { internalName = "cbc0ab3a-fc36-4ebe-97d2-75cc450ed2be" }, // Info icon column
                    new RollupListViewSettingsColumn { internalName = "c66e052d-6b51-4f66-935b-376f2e5163a9" } // Feedback Icon column
                }
        };

        private static PageRollupListViewSettings PageRollupDefaultListViewSettings = new PageRollupListViewSettings()
        {
            selectProperties = new List<string> { },
            columns = new List<RollupListViewSettingsColumn> {
                    new RollupListViewSettingsColumn { internalName = "07508398-980f-4412-ba74-c6dc82e2d1e6", isShowHeading = true } // Page Title & Link
            }
        };

        private static PageRollupListWithImageViewSettings PageRollupDefaultListViewWithImageSettings = new PageRollupListWithImageViewSettings()
        {
            selectProperties = new List<string> {
                Constants.BuiltInEnterpriseProperties.PageImage,
                Constants.BuiltInEnterpriseProperties.PageSummary,
                Constants.BuiltInEnterpriseProperties.ArticleDate,
                Constants.BuiltInEnterpriseProperties.ModifiedAt
            },
            imageProp = Constants.BuiltInEnterpriseProperties.PageImage,
            summaryProp = Constants.BuiltInEnterpriseProperties.PageSummary,
            dateProp = Constants.BuiltInEnterpriseProperties.ArticleDate
        };

        public static BannerBlockData MapBanner(G1BlockSetting srcBanner, WCMContextSettings wcmSettings)
        {
            if (srcBanner.ControlId != Constants.G1ControlIDs.Banner)
                return null;

            var bannerBlockData = new BannerBlockData();
            var g1BannerSettings = CloneHelper.Clone<G1BannerSetting>(srcBanner);
            if (g1BannerSettings.Settings.ViewId == null)
                g1BannerSettings.Settings.ViewId = "2B75C10B-BC52-4B44-88EC-9550474C8E7C"; // Image on top as default view

            //Diem - 02Aug2022: make sure the imageurl stored with root SP Url
            string imageURL = UrlHelper.MapUrl(g1BannerSettings.Settings.ImageUrl, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings);

            //Diem - 22Aug2022: make sure G1 link in banner content also be migrated
            string bannerContent = UrlHelper.MapAllUrlsInText(g1BannerSettings.Settings.Content, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings);

            MediaPropertyValue imageContent = null;
            BannerVideoData videoContent = null;

            if(g1BannerSettings.Settings.mediaData.IsNotNull())
            {
                try
                {//Diem - 03Aug2022: handle video in banner
                    if (g1BannerSettings.Settings.mediaData.isVideo == true)
                    {
                        videoContent = CloneHelper.Clone<BannerVideoData>(g1BannerSettings.Settings.mediaData.configuration);
                        imageURL = videoContent.thumbnailUrl;
                    }
                }
                catch (Exception ex)
                {
                    imageContent = EnterprisePropertyFactory.CreateDefaultMediaPropertyValue(imageURL);
                }
            }
            else
            {
                imageContent = EnterprisePropertyFactory.CreateDefaultMediaPropertyValue(imageURL);
            }

            int layout = 1;
            if (Guid.TryParse(g1BannerSettings.Settings.ViewId,out Guid ViewIdguid))
            {
                switch (ViewIdguid.ToString().ToUpper())
                {
                    case "535DE9AE-6CD0-4E61-B456-9628C5A0900A": // G1 View : Image Overlay (title on image)
                        layout = 3;
                        break;
                    case "8A7F1DF1-9E76-4FA0-81EC-046F2D6379C4": // G1 View : Image Overlay V2 (title and description on image)
                        layout = 2;
                        break;
                    case "2B75C10B-BC52-4B44-88EC-9550474C8E7C": // G1 View : Image on top
                    default:
                        layout = 1;
                        break;
                }
            }
            else
            {
                switch (g1BannerSettings.Settings.ViewId)
                {
                    case "omi-banner-template-imagetop":
                        layout = 1;
                        break;
                    case "omi-banner-template-imageright":
                        layout = 1;
                        break;
                    case "omi-banner-template-overlay":
                        layout = 3;
                        break;
                    case "omi-banner-template-overlayv2":
                        layout = 2;
                        break;
                    case "omi-banner-template-video":
                        layout = 1;
                        break;
                    default:
                        layout = 1;
                        break;
                }
            }

            string bannerTitle = g1BannerSettings.Settings.TitleSettings != null ? g1BannerSettings.Settings.TitleSettings.customTitle : 
                g1BannerSettings.Settings.Title;

            string bannerImageSvc = string.IsNullOrEmpty(imageURL) ? string.Empty :
                $"<img src='{imageURL}' style='max-width: 100%; width: 100%' alt='undefined'></img>";
            if (imageContent.IsNotNull())
            {
                bannerBlockData.Data = new BannerData
                {
                    title = bannerTitle,
                    content = bannerContent,
                    footer = g1BannerSettings.Settings.Footer,
                    //imagesrc = g1BannerSettings.Settings.ImageUrl,
                    imagesrc = imageURL,
                    imagesvg = bannerImageSvc,
                    layout = layout,
                    color = new BannerColorData
                    {
                        backgroundColor = g1BannerSettings.Settings.BackgroundColor,
                        contentColor = g1BannerSettings.Settings.ContentColor,
                        titleColor = g1BannerSettings.Settings.TitleColor,
                        footerColor = g1BannerSettings.Settings.ContentColor,
                    },
                    spacing = new BannerSpacing
                    {
                        top = 10,
                        bottom = 10,
                        left = 10,
                        right = 10
                    },
                    linkSetting = new BannerLinkSetting
                    {
                        link = new BannerLinkData
                        {
                            title = bannerTitle,
                            url = UrlHelper.MapUrl(g1BannerSettings.Settings.LinkUrl, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings),
                            openInNewWindow = g1BannerSettings.Settings.IsOpenLinkNewWindow
                        }
                    },
                    mediaContent = new BannerMediaData
                    {
                        imageContent = new BannerImageData
                        {
                            configuration = imageContent.configuration,
                            extraRatios = imageContent.ratios,
                            imageSrc = imageContent.src
                        }
                    }
                };
            }
            else
            {
                bannerBlockData.Data = new BannerVideoSettings
                {
                    title = bannerTitle,
                    content = bannerContent,
                    footer = g1BannerSettings.Settings.Footer,
                    layout = layout,
                    color = new BannerColorData
                    {
                        backgroundColor = g1BannerSettings.Settings.BackgroundColor,
                        contentColor = g1BannerSettings.Settings.ContentColor,
                        titleColor = g1BannerSettings.Settings.TitleColor,
                        footerColor = g1BannerSettings.Settings.ContentColor,
                    },
                    spacing = new BannerSpacing
                    {
                        top = 10,
                        bottom = 10,
                        left = 10,
                        right = 10
                    },
                    linkSetting = new BannerLinkSetting
                    {
                        link = new BannerLinkData
                        {
                            title = bannerTitle,
                            url = UrlHelper.MapUrl(g1BannerSettings.Settings.LinkUrl, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings),
                            openInNewWindow = g1BannerSettings.Settings.IsOpenLinkNewWindow
                        }
                    },
                    mediaContent = new BannerVideoData
                    {
                        html = videoContent.html,
                        videoUrl = videoContent.videoUrl,
                        mute = videoContent.mute,
                        autoPlay = videoContent.autoPlay,
                        thumbnailUrl = videoContent.thumbnailUrl
                    }
                };
            }
            return bannerBlockData;
        }

        public static PeopleRollupBlockData MapPeopleRollup(G1BlockSetting srcPeopleRollup, WCMContextSettings wcmSettings)
        {
            try
            {
                if (srcPeopleRollup.ControlId != Constants.G1ControlIDs.PeopleRollup)
                    return null;

                var peopleBlockData = new PeopleRollupBlockData();
                var g1PeopleSettings = CloneHelper.Clone<G1PeopleRollupSetting>(srcPeopleRollup).Settings;

                string blockTitle = g1PeopleSettings.titleSettings != null ? g1PeopleSettings.titleSettings.customTitle : g1PeopleSettings.title;
                VariationString g2BlockTitle = new VariationString();
                g2BlockTitle.Add(wcmSettings.CultureInfo, blockTitle);

                //  var g2RefinerPosition = MapRollupRefinerPosition(g1PeopleSettings.refinerLocation);

                int g2REFpostion = G2Rolluprefiner2(g1PeopleSettings.refinerLocation);








                // Transform query text to G2 format
                object g2Query = null;
                var g1QueryType = int.Parse(g1PeopleSettings.queryType);
                var g2QueryType = PeopleRollupQueryType.ProfileQuery;
                if (g1QueryType == 0)
                {
                    var g2QueryStr = g1PeopleSettings.queryData.ToString();
                    if (!string.IsNullOrEmpty(g2QueryStr))
                    {
                        foreach (var searchProp in wcmSettings.SearchProperties)
                        {
                            g2QueryStr = g2QueryStr.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property." + searchProp.G2PropertyName + "}");
                        }
                    }

                    g2Query = g2QueryStr;
                }
                else if (g1QueryType == 1)
                {
                    g2QueryType = PeopleRollupQueryType.SharePointGroups;
                    g2Query = CloneHelper.Clone<List<string>>(g1PeopleSettings.queryData);
                }
                else if (g1QueryType == 2)
                {
                    g2QueryType = PeopleRollupQueryType.PageQuery;
                    var g1QueryProps = CloneHelper.Clone<List<string>>(g1PeopleSettings.queryData);
                    var g2QueryProps = new List<string>();
                    foreach (var prop in g1QueryProps)
                    {
                        if (wcmSettings.EnterprisePropertiesMappings.ContainsKey(prop))
                        {
                            g2QueryProps.Add(wcmSettings.EnterprisePropertiesMappings[prop].PropertyName);
                        }
                    }
                    g2Query = g2QueryProps;
                }

                // Map sortby property by managed property
                var g2SortBy = ""; // Default sortby property Thoan change 29/06/2023




                if (!string.IsNullOrEmpty(g1PeopleSettings.sortByProperty))
                {
                    var sortPropId = new Guid(g1PeopleSettings.sortByProperty);
                    var sortbySearchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.G1PropertyId == sortPropId);
                    if (sortbySearchProp != null)
                    {
                        g2SortBy = sortbySearchProp.G2PropertyName;
                    }
                }

                // Map refiners by managed property
                var g2Refiners = new List<RollupRefiner>();
                if (g1PeopleSettings.refiners != null)
                {
                    foreach (var g1Refiner in g1PeopleSettings.refiners)
                    {
                        var searchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.ManagedPropertyName == g1Refiner.managedProperty || x.G1PropertyId == g1Refiner.id);
                        if (searchProp != null)
                        {

                            var g1RefinerRefinerOrderBy = g1Refiner.refinerOrderBy ?? default(int);
                            var g1RefinerRefinerLimit = g1Refiner.refinerLimit ?? default(int);
                            g2Refiners.Add(new RollupRefiner
                            {
                                refinerOrderBy = g1RefinerRefinerOrderBy,
                                refinerLimit = g1RefinerRefinerLimit,
                            property = searchProp.G2PropertyName
                            });
                        }
                    }
                }


                var g2Filters = new List<RollupFilter>();

                // Use default view settings for now
                var g2ViewSettings = new PeopleRollupCardViewSettings
                {
                    showProfileImage = g1PeopleSettings.showProfileImage,
                    //borderColor = g1PeopleSettings.borderColor,
                    //borderRadius = g1PeopleSettings.borderRadius,
                    //borderWidth = g1PeopleSettings.borderWidth,
                    //elevation = g1PeopleSettings.elevation,
                    //June 2023
                   // totalColumns = 1,
                   


                };








                g2ViewSettings.selectProperties.Add(wcmSettings.DefaultPeopleNameProperty);
                if (g1PeopleSettings.selectedProperties != null)
                {
                    foreach (var prop in g1PeopleSettings.selectedProperties)
                    {
                        var g2Prop = wcmSettings.SearchProperties.FirstOrDefault(y => y.G1PropertyId == prop.id);
                        if (!string.IsNullOrEmpty(g2Prop?.G2PropertyName))
                        {
                            var g2PropertyName = g2Prop.G2PropertyName;

                           g2ViewSettings.selectProperties.Add(g2PropertyName);
                            g2ViewSettings.columns.Add(new PeopleRollupCardViewSettingsColumn
                            {
                                internalName = g2PropertyName,
                                type = 1,
                                // themeColor = prop.themeColor
                            });
                        }
                    }
                }
               // g2ViewSettings.columns.Clear();

              //  g2ViewSettings.avatarSize = 48;


                var G2pagingType = RollupPagingType.Classic;

                if (g1PeopleSettings.pagingType.ToString() == "2")
                {
                    G2pagingType = RollupPagingType.Scroll;
                }


                g2ViewSettings.name = wcmSettings.DefaultPeopleNameProperty;

             
                peopleBlockData.Settings = new PeopleRollupBlockSetting
                {
                    title = g2BlockTitle,
                    itemLimit = g1PeopleSettings.pageSize,
                    pagingType = G2pagingType,
                    sortby = g2SortBy, //Diem -31102022: replace "sortBy"
                    sortDescending = g1PeopleSettings.sortByDirection == 1,
                    refinerPosition = 1,
                    refiners = g2Refiners,
                    filterPosition = RollupRefinerPositions.Top,
                    filters = g2Filters,
                    selectedViewId = new Guid(PeopleRollupDefaultView), // Default view
                    viewSettings = g2ViewSettings,
                    showSearchBox = g1PeopleSettings.showSearchBox ?? false,
                    query = g2Query,
                    queryType = g2QueryType,
                    totalColumns= g1PeopleSettings.totalColumns,
                

                };

                return peopleBlockData;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public static DocumentRollupBlockData MapControlledDocumentView(G1BlockSetting srcDocumentRollup, WCMContextSettings wcmSettings)
        {
            if (srcDocumentRollup.ControlId != Constants.G1ControlIDs.ControlledDocumentView)
                return null;

            var documentsBlockData = new DocumentRollupBlockData();
            var g1DocumentSettings = CloneHelper.Clone<G1ControlledDocumentViewSettings>(srcDocumentRollup).Settings;
           



            string blockTitle = g1DocumentSettings.titleSettings != null ? g1DocumentSettings.titleSettings.customTitle : g1DocumentSettings.title;
            VariationString g2BlockTitle = new VariationString();
            g2BlockTitle.Add(wcmSettings.CultureInfo, blockTitle);

            var g2RefinerPosition = MapRollupRefinerPosition(g1DocumentSettings.refinerLocation);
            var g2FilterLocation= MapRollupRefinerPosition(g1DocumentSettings.filterLocation);

            // Transform query text to G2 format
            var g2Query = g1DocumentSettings.queryText;
            if (!string.IsNullOrEmpty(g2Query))
            {
                foreach (var searchProp in wcmSettings.SearchProperties)
                {
                    /**********************************************************/
                    if (searchProp.G1PropertyName=="Titel")
                    {
                        g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property.title}");
                        continue;
                    }
                    if (searchProp.G1PropertyName == "Forvaltning")
                    {
                        g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property.odmp_F_x00f6_rvaltning}");
                        continue;
                    }
                    /**********************************************************/
                    g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property." + searchProp.G2PropertyName + "}");
                }

                g2Query = g2Query.Replace("{Site}=", "path:");
            }

            // Map sortby property by managed property
            //var g2SortBy = DocumentRollupDefaultSortBy;
            string g2SortBy = "";
            var sortbySearchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.G1PropertyName == g1DocumentSettings.sortByProperty);
          
            if (sortbySearchProp != null)
            {
                g2SortBy = sortbySearchProp.G2PropertyName;
            }
          //  else { g2SortBy = "title"; }
            
            // Map refiners by managed property
            var g2Refiners = new List<RollupRefiner>();
            if (g1DocumentSettings.refiners != null)
            {
                foreach (var g1Refiner in g1DocumentSettings.refiners)
                {
                    var searchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.ManagedPropertyName == g1Refiner.managedProperty);
                    if (searchProp != null)
                    {
                        var g1RefinerRefinerOrderBy = g1Refiner.refinerOrderBy ?? default(int);
                        var g1RefinerRefinerLimit = g1Refiner.refinerLimit ?? default(int);
                        g2Refiners.Add(new RollupRefiner
                        {
                            // refinerOrderBy = g1RefinerRefinerOrderBy,
                            refinerOrderBy = 1,
                            refinerLimit = g1RefinerRefinerLimit,
                            property = searchProp.G2PropertyName
                        }) ;
                    }
                }
            }

            var g2Filters = new List<RollupFilter>();
            if (g1DocumentSettings.showSearchBox == true){
                g2Filters.Add(new RollupFilter { property = "5ee3f203-fd1e-41d1-97e5-dcf21bbf726d" }); // Default search box filter
            }

            // Use default view settings for now
            var g2ViewSettings = DocumentRollupDefaultViewSettings;
            //var g2ViewSettingsSun = new DocumentRollupListViewSettings();
            ////Sun test column
            //g2ViewSettingsSun.selectProperties = new List<string> { "title", "FileName", "ODMDocId" };
            //g2ViewSettingsSun.columns = new List<RollupListViewSettingsColumn>{
            //new RollupListViewSettingsColumn { internalName = "b18ac29d-b579-4971-ac5e-c0f71ad33ae1" },
            //new RollupListViewSettingsColumn { internalName = "ODMDocId",type= 1,isShowHeading= true }
            //};


            var g2ViewSettingsNew = new DocumentRollupListViewSettings();
            g2ViewSettingsNew.selectProperties = new List<string> {  };
            g2ViewSettingsNew.columns = new List<RollupListViewSettingsColumn> { };

            foreach (var col in g1DocumentSettings.columns)
            {
                if (col.displayNameInText == "Icon, Title and Link" || col.displayNameInText == "Ikon, titel och länk" || col.displayNameInText == "Title") {
                    // Thêm title

                    //  g2ViewSettingsNew.selectProperties.Add("title");
                    g2ViewSettingsNew.selectProperties.Add("title");
                    g2ViewSettingsNew.selectProperties.Add("Filename");

                    var titleCol = new RollupListViewSettingsColumn { internalName = "cd49fdbc-ea98-4bfd-ad97-f471b68c4505", isShowHeading = true };                   
                    g2ViewSettingsNew.columns.Add(titleCol);

                }
                else if (col.displayNameInText== "Info Icon")

                {
                    var infoIconCol = new RollupListViewSettingsColumn { internalName = "cbc0ab3a-fc36-4ebe-97d2-75cc450ed2be", isShowHeading = true };
                   
                    g2ViewSettingsNew.columns.Add(infoIconCol);

                }
                else if (col.displayNameInText == "Feedback Icon")
                {
                    g2ViewSettingsNew.selectProperties.Add("c66e052d-6b51-4f66-935b-376f2e5163a9");
                  
                    var feedbackIconCol = new RollupListViewSettingsColumn { internalName = "c66e052d-6b51-4f66-935b-376f2e5163a9", isShowHeading = true };
                   
                    g2ViewSettingsNew.columns.Add(feedbackIconCol);

                }
                else if (col.displayNameInText == "Related Documents Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("ODMRelatedDocuments");
                    var relatedDocIconCol = new RollupListViewSettingsColumn { internalName = "86772b00-de78-40f9-94cb-2337e08feffb", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(relatedDocIconCol);

                }
                else if (col.displayNameInText == "Create Document Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("df12a2fc-0f46-483c-932f-2dee33eded91");
                    var createDocCol = new RollupListViewSettingsColumn { internalName = "df12a2fc-0f46-483c-932f-2dee33eded91", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(createDocCol);

                }
                else if (col.displayNameInText == "Appendices Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("ODMAppendices");
                    var ApenCol = new RollupListViewSettingsColumn { internalName = "a4d0d69d-7ad2-4099-b2f6-717927064f63", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(ApenCol);

                }

                else
                {
                    var searchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.G1PropertyId == col.id);
                    if (searchProp != null)
                    {
                        g2ViewSettingsNew.selectProperties.Add(searchProp.G2PropertyName);
                        
                            var normalCol = new RollupListViewSettingsColumn { internalName = searchProp.G2PropertyName, type = searchProp.type, isShowHeading = true };
                        if (searchProp.type == (int)PropertyIndexedType.DateTime)
                        {
                            if (col.formatting == 2)
                                normalCol.mode = (int)DateTimeMode.Normal;
                            else normalCol.mode = (int)DateTimeMode.Social;
                        }
                        //width for IKEA
                        //if (searchProp.G2PropertyName == "ODMPublished")

                        //{
                        //    normalCol.width = 110;
                            
                        //}
                        
                            g2ViewSettingsNew.columns.Add(normalCol);

                    }
                }
                




            }

            //var G1pagingType =g1DocumentSettings.pagingStyle;
            var G2pagingType = RollupPagingType.Classic;
            if (g1DocumentSettings.pagingStyle.ToString()=="2")
            {
                G2pagingType = RollupPagingType.Scroll;
            }


            //pagingType = RollupPagingType.Classic,
           // filterPosition = RollupRefinerPositions.Top,
            documentsBlockData.Settings = new DocumentRollupBlockSetting
            {
                title = g2BlockTitle,
                itemLimit = g1DocumentSettings.pageSize,
                pagingType = G2pagingType,
                sortby = g2SortBy,//Diem - 31102022: replace "sortBy"
                sortDescending = g1DocumentSettings.sortByDirection == 1,
                openInClientApp = !g1DocumentSettings.isOpenInOffice,
                refinerPosition = g2RefinerPosition,
                refiners = g2Refiners,
                filterPosition = g2FilterLocation,
                filters = g2Filters,
                searchScope = (DocumentRollupQueryScope)g1DocumentSettings.searchScope,
                selectedViewId = new Guid(DocumentRollupDefaultView),
                viewSettings = g2ViewSettingsNew,
                query = g2Query,
            };
            // viewSettings = g2ViewSettings,

            return documentsBlockData;
        }

        public static DocumentRollupBlockData MapDocumentRollup(G1BlockSetting srcDocumentRollup, WCMContextSettings wcmSettings)
        {
            if (srcDocumentRollup.ControlId != Constants.G1ControlIDs.DocumentRollup)
                return null;

            var documentsBlockData = new DocumentRollupBlockData();
            var g1DocumentSettings = CloneHelper.Clone<G1DocumentRollupSetting>(srcDocumentRollup).Settings;

            string blockTitle = g1DocumentSettings.titleSettings != null ? g1DocumentSettings.titleSettings.customTitle : g1DocumentSettings.title;
            VariationString g2BlockTitle = new VariationString();
            g2BlockTitle.Add(wcmSettings.CultureInfo, blockTitle);

            var g2RefinerPosition = MapRollupRefinerPosition(g1DocumentSettings.refinerLocation);
            var g2FilterLocation = MapRollupRefinerPosition(g1DocumentSettings.filterLocation);

            // Transform query text to G2 format
            var g2Query = g1DocumentSettings.queryText;
            if (!string.IsNullOrEmpty(g2Query))
            {
                foreach (var searchProp in wcmSettings.SearchProperties)
                {
                    /**********************************************************/
                    if (searchProp.G1PropertyName == "Titel")
                    {
                        g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property.title}");
                        continue;
                    }
                    if (searchProp.G1PropertyName == "Forvaltning")
                    {
                        g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property.odmp_F_x00f6_rvaltning}");
                        continue;
                    }
                    /**********************************************************/
                    g2Query = g2Query.Replace("{Property." + searchProp.G1PropertyName + "}", "{Property." + searchProp.G2PropertyName + "}");
                }

                g2Query = g2Query.Replace("{Site}=", "path:");
            }

            // Map sortby property by managed property
            //var g2SortBy = DocumentRollupDefaultSortBy;
            string g2SortBy = "";
            var sortbySearchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.ManagedPropertyName == g1DocumentSettings.sortByProperty?.managedProperty);
            if (sortbySearchProp != null)
            {
                g2SortBy = sortbySearchProp.G2PropertyName;
            }
           
            // Map refiners by managed property
            var g2Refiners = new List<RollupRefiner>();
            if (g1DocumentSettings.refiners != null)
            {
                foreach (var g1Refiner in g1DocumentSettings.refiners)
                {
                    var searchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.ManagedPropertyName == g1Refiner.managedProperty);
                    if (searchProp != null)
                    {
                        var g1RefinerRefinerOrderBy = g1Refiner.refinerOrderBy ?? default(int);
                        var g1RefinerRefinerLimit = g1Refiner.refinerLimit ?? default(int);
                        g2Refiners.Add(new RollupRefiner
                        {
                            refinerOrderBy = g1RefinerRefinerOrderBy,
                            refinerLimit = g1RefinerRefinerLimit ,
                            property = searchProp.G2PropertyName
                        });
                    }
                }
            }

            var g2Filters = new List<RollupFilter>();
            if (g1DocumentSettings.showSearchBox == true)
            {
                g2Filters.Add(new RollupFilter { property = "5ee3f203-fd1e-41d1-97e5-dcf21bbf726d" }); // Default search box filter
            }

            // Use default view settings for now
            var g2ViewSettings = DocumentRollupDefaultViewSettings;
            var g2ViewSettingsNew = new DocumentRollupListViewSettings();
            g2ViewSettingsNew.selectProperties = new List<string> { };
            g2ViewSettingsNew.columns = new List<RollupListViewSettingsColumn> { };
            foreach (var col in g1DocumentSettings.columns)
            {
                if (col.displayNameInText == "Icon, Title and Link" || col.displayNameInText == "Title")
                {
                    // Thêm title

                    //  g2ViewSettingsNew.selectProperties.Add("title");
                    g2ViewSettingsNew.selectProperties.Add("title");
                    g2ViewSettingsNew.selectProperties.Add("Filename");

                    var titleCol = new RollupListViewSettingsColumn { internalName = "cd49fdbc-ea98-4bfd-ad97-f471b68c4505", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(titleCol);

                }
                else if (col.displayNameInText == "Info Icon")

                {
                    var infoIconCol = new RollupListViewSettingsColumn { internalName = "cbc0ab3a-fc36-4ebe-97d2-75cc450ed2be", isShowHeading = true };

                    g2ViewSettingsNew.columns.Add(infoIconCol);

                }
                else if (col.displayNameInText == "Feedback Icon")
                {
                    g2ViewSettingsNew.selectProperties.Add("c66e052d-6b51-4f66-935b-376f2e5163a9");

                    var feedbackIconCol = new RollupListViewSettingsColumn { internalName = "c66e052d-6b51-4f66-935b-376f2e5163a9", isShowHeading = true };

                    g2ViewSettingsNew.columns.Add(feedbackIconCol);

                }
                else if (col.displayNameInText == "Related Documents Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("ODMRelatedDocuments");
                    var relatedDocIconCol = new RollupListViewSettingsColumn { internalName = "86772b00-de78-40f9-94cb-2337e08feffb", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(relatedDocIconCol);

                }
                else if (col.displayNameInText == "Create Document Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("df12a2fc-0f46-483c-932f-2dee33eded91");
                    var createDocCol = new RollupListViewSettingsColumn { internalName = "df12a2fc-0f46-483c-932f-2dee33eded91", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(createDocCol);

                }
                else if (col.displayNameInText == "Appendices Icon")
                {
                    //internalName = "ODMRelatedDocuments"
                    g2ViewSettingsNew.selectProperties.Add("ODMAppendices");
                    var ApenCol = new RollupListViewSettingsColumn { internalName = "a4d0d69d-7ad2-4099-b2f6-717927064f63", isShowHeading = true };
                    g2ViewSettingsNew.columns.Add(ApenCol);

                }

                else
                {
                    var searchProp = wcmSettings.SearchProperties.FirstOrDefault(x => x.G1PropertyId == col.id);
                    if (searchProp != null)
                    {
                        g2ViewSettingsNew.selectProperties.Add(searchProp.G2PropertyName);

                        var normalCol = new RollupListViewSettingsColumn { internalName = searchProp.G2PropertyName, type = searchProp.type, isShowHeading = true };
                        if (searchProp.type == (int)PropertyIndexedType.DateTime)
                        {
                            if (col.formatting == 2)
                                normalCol.mode = (int)DateTimeMode.Normal;
                            else normalCol.mode = (int)DateTimeMode.Social;


                        }
                       
                       

                        g2ViewSettingsNew.columns.Add(normalCol);

                    }
                }





            }

            var G2pagingType = RollupPagingType.Classic;
            if (g1DocumentSettings.pagingStyle.ToString() == "2")
            {
                G2pagingType = RollupPagingType.Scroll;
            }


            documentsBlockData.Settings = new DocumentRollupBlockSetting
            {
                title = g2BlockTitle,
                itemLimit = g1DocumentSettings.pageSize,
                pagingType = G2pagingType,
                sortby = g2SortBy,//Diem - 31102022: replace "sortBy"
                sortDescending = g1DocumentSettings.sortByDirection == 1,
                openInClientApp = !g1DocumentSettings.isOpenInOffice,
                refinerPosition = g2RefinerPosition,
                refiners = g2Refiners,
                filterPosition = g2FilterLocation,
                filters = g2Filters,
                searchScope = (DocumentRollupQueryScope)g1DocumentSettings.searchScope, //fix for intranet document rollup
                selectedViewId = new Guid(DocumentRollupDefaultView), // Default view
                viewSettings = g2ViewSettingsNew,
                query = g2Query
            };

            return documentsBlockData;
        }

        public static PageRollupBlockData MapNewsViewer(G1BlockSetting srcNewsViewer, WCMContextSettings wcmSettings)
        {
            if (srcNewsViewer.ControlId != Constants.G1ControlIDs.NewsViewer)
                return null;

            var pageRollupBlockData = new PageRollupBlockData();
            var g1NewsViewerSettings = CloneHelper.Clone<G1NewsViewerSetting>(srcNewsViewer).Settings;

            // Use default view settings for now
            PageRollupViewSettings g2ViewSettings;
            Guid g2ViewId = Guid.Empty;
            switch (g1NewsViewerSettings.view)
            {

                case Constants.G1BlockViewIDs.NewsViewer.ListViewV2:
                    g2ViewSettings = PageRollupDefaultListViewWithImageSettings;
                    g2ViewId = Constants.BlockViewIDs.PageRollup.ListViewWithImage;
                    break;
                case Constants.G1BlockViewIDs.NewsViewer.SimpleListView:
                default:
                    g2ViewSettings = PageRollupDefaultListViewSettings;
                    g2ViewId = Constants.BlockViewIDs.PageRollup.ListView;
                    break;
            }

            string g2ItemLimtit = "10"; //default
            var g2Resources = new List<PageRollupResource>();
            if (g1NewsViewerSettings.query != null)
            {
                g2ItemLimtit = g1NewsViewerSettings.query.itemLimit.ToString();

                foreach (var newsCenter in g1NewsViewerSettings.query.newsCenterQuery)
                {
                    var newsCenterUrl = newsCenter.newsCenterUrl.ToLower();
                    var pageCollectionId = wcmSettings.NewsCenterMappings.FirstOrDefault(mapping => newsCenterUrl.Contains(mapping.NewsCenterUrl.ToLower()))?.PageCollectionId;
                    if (pageCollectionId != null)
                    {
                        var g2Resource = new PageRollupResource { id = pageCollectionId.ToString() };
                        g2Resources.Add(g2Resource);

                        var g1Filters = newsCenter.filters.Where(x => wcmSettings.EnterprisePropertiesMappings.ContainsKey(x.fieldName)).ToList();

                        foreach (var g1Filter in g1Filters)
                        {
                            var propertyName = "Prop" + wcmSettings.EnterprisePropertiesMappings[g1Filter.fieldName].PropertyName;
                            var g2Filter = new PageRollupFilter
                            {
                                property = propertyName
                            };
                            switch (g1Filter.typeAsString)
                            {
                                case "TaxonomyFieldTypeMulti":
                                    var valueObj = new PageRollupFilterTaxonomyValue();
                                    if (g1Filter.filterType == 1)
                                    {
                                        valueObj.filterType = 1;
                                        if (g1Filter.taxonomyValue != null)
                                        {
                                            valueObj.fixedTermIds = g1Filter.taxonomyValue.Select(x => x.Id).ToList();
                                        }
                                        else
                                        {
                                            valueObj.fixedTermIds = g1Filter.taxonomyValues;
                                        }
                                    }
                                    else if (g1Filter.filterType == 2)
                                    {
                                        valueObj.filterType = 3;
                                    }

                                    valueObj.includeChildTerms = g1Filter.includeChildTerms;
                                    valueObj.includeEmpty = g1Filter.includeEmpty;
                                    g2Filter.type = 6;
                                    g2Filter.valueObj = valueObj;
                                    break;
                                case "Boolean":
                                    g2Filter.type = 4;
                                    g2Filter.valueObj = new PageRollupFilterBooleanValue { value = bool.Parse(g1Filter.value.ToString()) };
                                    break;
                            }

                            g2Resource.filters.Add(g2Filter);
                        }
                    }


                }
            }

            pageRollupBlockData.Settings = new PageRollupBlockSetting
            {
                resources = g2Resources,
                itemLimit = g2ItemLimtit,
                pagingType = RollupPagingType.Classic,
                selectedViewId = g2ViewId,
                viewSettings = g2ViewSettings,
                scope = 3 // hard-coded to "page collection" scope
            };

            return pageRollupBlockData;
        }

        private static ScriptHtmlBlockData MapScriptHtml(G1BlockSetting srcScriptHtml, WCMContextSettings wcmSettings)
        {
            if (srcScriptHtml.ControlId != Constants.G1ControlIDs.ScriptHtml)
                return null;

            var scriptHtmlBlockData = new ScriptHtmlBlockData();
            var g1ScriptHtmlV2Settings = CloneHelper.Clone<G1ScriptHtmlV2Setting>(srcScriptHtml).Settings;

            if (g1ScriptHtmlV2Settings.data != null)
            {
                scriptHtmlBlockData.Data = new ScriptHtmlData
                {
                    html = g1ScriptHtmlV2Settings.data.html,
                    js = g1ScriptHtmlV2Settings.data.js,
                    css = g1ScriptHtmlV2Settings.data.css,
                    hiddenBlock = g1ScriptHtmlV2Settings.data.hiddenBlock,
                    runInIframe = g1ScriptHtmlV2Settings.data.runInIframe,
                    runScriptInEditMode = g1ScriptHtmlV2Settings.data.runScriptInEditMode,
                };
            }
            else
            {
                var g1ScriptHtmlV1Settings = CloneHelper.Clone<G1ScriptHtmlV1Setting>(srcScriptHtml).Settings;
                scriptHtmlBlockData.Data = new ScriptHtmlData
                {
                    html = g1ScriptHtmlV1Settings.content
                };
            }

            return scriptHtmlBlockData;
        }

        private static AccordionBlockData MapAccordion(G1BlockSetting srcAccordion, WCMContextSettings wcmSettings)
        {
            if (srcAccordion.ControlId != Constants.G1ControlIDs.Accordion)
                return null;

            var accordionBlockData = new AccordionBlockData();
            var g1AccordionBlockDataSettings = CloneHelper.Clone<G1AccordionSetting>(srcAccordion).Settings;

            var accordions = new List<AccordionDataItem>();

            for (int i = 0; i < g1AccordionBlockDataSettings.blocks.Count; i++)
            {
                var accordionBlock = g1AccordionBlockDataSettings.blocks[i];
                accordions.Add(new AccordionDataItem
                {
                    content = UrlHelper.MapAllUrlsInText(
                        accordionBlock.content,
                        wcmSettings.SharePointUrl, wcmSettings.
                        SharePointLocationMappings),
                    title = accordionBlock.title,
                    id = i
                });
            }

            accordionBlockData.Data = new AccordionData { accordions = accordions };

            return accordionBlockData;
        }
        public static SVGViewerBlockData MapSVGViewer(G1BlockSetting srcSVGViewer, WCMContextSettings wcmSettings)
        {
            if (srcSVGViewer.ControlId != Constants.G1ControlIDs.SVGViewer)
                return null;

            var svgViewerBlockData = new SVGViewerBlockData();
            var g1SVGViewerSettings = CloneHelper.Clone<G1SVGViewerSetting>(srcSVGViewer).Settings;
            
            VariationString g2BlockTitle = new VariationString();
            g2BlockTitle.Add(wcmSettings.CultureInfo, null);

            string imageSrc = g1SVGViewerSettings.url != null ? g1SVGViewerSettings.url : g1SVGViewerSettings.tempurl !=null? g1SVGViewerSettings.tempurl : "";
            if(imageSrc.IsNull())
            {
                return null;
            }
            var imageFileName = Path.GetFileName(imageSrc).Split("?")[0];
            var imgParts = imageSrc.Split("/");
            var imgPath = imgParts[0] + "//" + imgParts[2] + "/" + imgParts[3] + "/" + imgParts[4];

            svgViewerBlockData.Settings = new SVGViewerBlockSetting
            {
                enableDownloadButton = g1SVGViewerSettings.showDownload,
                //pageProperty = wcmSettings.DefaultSVGViewerProperty
                blockTitle = g2BlockTitle,
                spacing = new SVGSpacing
                {
                    top = 0,
                    bottom = 0,
                    left = 0,
                    right = 0
                },
                svgImage = new SVGViewerData
                {
                    documentUrl = g1SVGViewerSettings.url,
                    format = "svg",
                    name= imageFileName.Split(".svg").First(),
                    spWebUrl =imgPath
                }
            };            
            svgViewerBlockData.Data = new SVGViewerData
            {
                documentUrl = g1SVGViewerSettings.url,
                format = "svg",
                name = imageFileName.Split(".svg").First(),
                spWebUrl = imgPath
            };   

            return svgViewerBlockData;
        }
        public static RSSBlockData MaprRSSReader(G1BlockSetting srcRSSReader, WCMContextSettings wcmSettings)
        {
            if (srcRSSReader.ControlId != Constants.G1ControlIDs.RSSBlock)
                return null;

            var rssBlockData = new RSSBlockData();
            var g1RSSSettings = CloneHelper.Clone<G1RSSSetting>(srcRSSReader).Settings;

            Guid g2ViewId = Guid.Empty;
            switch (g1RSSSettings.viewType)
            {
                case 0:
                    g2ViewId = Constants.BlockViewIDs.RSSViewType.ImageOnLeft;
                    break;
                case 1:
                    g2ViewId = Constants.BlockViewIDs.RSSViewType.ImageOnRight;
                    break;
                default:
                    g2ViewId = Constants.BlockViewIDs.RSSViewType.NoImageDisplayed;
                    break;
            }
            string blockTitle = g1RSSSettings.titleSettings != null ? g1RSSSettings.titleSettings.customTitle : g1RSSSettings.title;
            VariationString g2BlockTitle = new VariationString();
            g2BlockTitle.Add(wcmSettings.CultureInfo, blockTitle);

            rssBlockData.Settings = new RSSBlockSetting
            {
                selectedViewId = g2ViewId,
                source = g1RSSSettings.source,
                spacing = null,
                itemLimit = g1RSSSettings.pageSize,
                isOpenNewWindow = g1RSSSettings.isOpenNewWindow,
                showActualDay = false,
                showTitle = g1RSSSettings.showTitle,
                pagingType = RollupPagingType.NoPaging,
                title = g2BlockTitle,
                viewSettings=new RSSViewSettings()
            };
            return rssBlockData;
        }
        public static BaseBlockData MapBlockData(G1BlockSetting srcBlockSetting, WCMContextSettings wcmSettings)
        {
            var srcControlId = srcBlockSetting.ControlId.ToString().ToUpper();
            switch (srcControlId)
            {
                case Constants.G1ControlIDs.BannerIdString:
                    return MapBanner(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.PeopleRollupIdString:
                    return MapPeopleRollup(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.DocumentRollupIdString:
                    return MapDocumentRollup(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.ControlledDocumentViewIdString:
                    return MapControlledDocumentView(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.NewsViewerIdString:
                    return MapNewsViewer(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.ScriptHtmlIdString:
                    return MapScriptHtml(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.AccordionIdString:
                    return MapAccordion(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.SVGViewerIdString:
                    return MapSVGViewer(srcBlockSetting, wcmSettings);
                case Constants.G1ControlIDs.RSSBlockIdString:
                    return MaprRSSReader(srcBlockSetting, wcmSettings);
                default:
                    return null;
            }
        }

        private static RollupRefinerPositions MapRollupRefinerPosition(int? g1RefinerPosition)
        {
            var g2RefinerPosition = RollupRefinerPositions.Top;

            switch (g1RefinerPosition)
            {
                case 1:
                    g2RefinerPosition = RollupRefinerPositions.Left;// Thoan sửa
                    break;
                case 2:
                    g2RefinerPosition = RollupRefinerPositions.Right;
                    break;
                case 3:
                default:
                    g2RefinerPosition = RollupRefinerPositions.Top;
                    break;
            }

            return g2RefinerPosition;
        }
       private static int G2Rolluprefiner2(int? g1RefinerPosition)

        {

            if (g1RefinerPosition == 1) return 2;
            if (g1RefinerPosition == 2) return 3;
            if (g1RefinerPosition == 3) return 1;
            else return 1;
        }


    }
}

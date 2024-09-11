using Omnia.Fx.Models.Language;
using Omnia.Fx.Utilities;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models.BlockData;
using Omnia.Migration.Models.Input.Links;
using Omnia.Migration.Models.Links;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Mappers
{
    public static class LinkMapper
    {
        //bold icon
        //private static string DefaultFontAwesomeClass = "fas fa-link";
        private static string DefaultFontAwesomeClass = "fal fa-link";
        public static RelatedLink MapRelatedLink(RelatedLink oldValue, WCMContextSettings wcmSettings)
        {
            var newLink = new RelatedLink
            {
                icon = oldValue.linkType != RelatedLinkTypes.Heading ? new RelatedLinkIcon() { iconType = "unknown" } : null,
                index = oldValue.index,
                linkType = oldValue.linkType == RelatedLinkTypes.PageLink ? RelatedLinkTypes.CustomLink : oldValue.linkType,
                title = oldValue.title,
                openInNewWindow = oldValue.openInNewWindow
            };

            newLink.url = UrlHelper.MapUrl(oldValue.url, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings);

            return newLink;
        }

        public static QuickLink MapSharedLink(G1CommonLink oldValue, WCMContextSettings wcmSettings, string iconColor, string backgroundColor)
        {
            QuickLink newLink = null;
            if (oldValue.GetType() == typeof(G1MyLink))
            {
                var temp = oldValue as G1MyLink;
                if (temp.UserLoginName.StartsWith("i:0#.f|membership|"))
                {
                    temp.UserLoginName = temp.UserLoginName.Replace("i:0#.f|membership|", "");
                }
                newLink = new QuickLink
                {
                    Title = new MultilingualString(),
                    Category = oldValue.Category,
                    Url = oldValue.Url,
                    Mandatory = oldValue.Mandatory,
                    IsOpenNewWindow = oldValue.IsOpenNewWindow,
                    Information = new MultilingualString(),
                    UserLoginName = temp.UserLoginName
                };
            }
            else
            {
                newLink = new QuickLink
                {
                    Title = new MultilingualString(),
                    Category = oldValue.Category,
                    Url = oldValue.Url,
                    Mandatory = oldValue.Mandatory,
                    IsOpenNewWindow = oldValue.IsOpenNewWindow,
                    Information = new MultilingualString(),
                };
            }          

            var language = CultureUtils.GetCultureInfo(wcmSettings.CultureInfo);
            newLink.Title.Add(language.Name, oldValue.Title);
            newLink.Information.Add(language.Name, oldValue.Information);
            if (backgroundColor.IsNotNull()&& oldValue.Icon.BackgroundColor == null)
            {
                oldValue.Icon.BackgroundColor = backgroundColor;
            }

            if (oldValue.Icon != null)
            {
                newLink.Icon = new LinkIcon()
                {
                    iconType = oldValue.Icon.IconType == G1IconType.Font ? LinkIconTypes.FontAwesome : LinkIconTypes.Custom,
                    faClass = oldValue.Icon.IconType == G1IconType.Font ? MapFontAwesomeClass(oldValue.Icon.FontValue) : DefaultFontAwesomeClass,
                    customValue = oldValue.Icon.CustomValue,
                    backgroundColor = oldValue.Icon.BackgroundColor,                    
                    color = iconColor
                };
            }
            else
            {
                newLink.Icon = new LinkIcon()
                {
                    iconType = LinkIconTypes.FontAwesome,
                    faClass = DefaultFontAwesomeClass,
                    customValue = string.Empty,
                    backgroundColor = "#fff",
                    color = "#000"
                };
            }

            newLink.Url = UrlHelper.MapUrl(oldValue.Url, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings,true);
            var customIconValue = newLink.Icon?.customValue;
            if (!string.IsNullOrEmpty(customIconValue) &&
                customIconValue.StartsWith(wcmSettings.SharePointUrl) && 
                UrlHelper.IsImageUrl(customIconValue))
            {
                newLink.Icon.customValue = "/api/webimage/previewproxy?cache=true&previewUrl=" + customIconValue;
            }

            return newLink;
        }

        private static string MapFontAwesomeClass(string oldClass)
        {
            if (string.IsNullOrEmpty(oldClass))
                return oldClass;
            if (oldClass.IndexOf("fa ") > -1)
                //return oldClass.Replace("fa ", "fas "); (old code make link icon bold)
                return oldClass.Replace("fa ", "fal ");
            else
                //return "fas " + oldClass; (old code make link icon bold)
                return "fal " + oldClass;
        }

        
    }
}

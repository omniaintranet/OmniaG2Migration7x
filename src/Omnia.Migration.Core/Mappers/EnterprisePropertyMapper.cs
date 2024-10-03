using Microsoft.Office.SharePoint.Tools;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Factories;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.EnterpriseProperties;
using Omnia.Migration.Models.Input.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UrlCombineLib;

namespace Omnia.Migration.Core.Mappers
{
    public class identity
    {

        public Guid id { get;  set; }

        //
        // Summary:
        //     The type of identity
        public IdentityTypes type { get; set; }
    }
    public static class EnterprisePropertyMapper
    {
        public static JToken MapPropertyValue(JToken oldValue, EnterprisePropertyType propertyType, WCMContextSettings wcmSettings, ItemQueryResult<IResolvedIdentity> Identities)
        {
            switch (propertyType)
            {
                case EnterprisePropertyType.Text:
                case EnterprisePropertyType.MainContent:
                    return MapTextPropertyValue(oldValue, wcmSettings);
                case EnterprisePropertyType.Image:
                    return MapImagePropertyValue(oldValue, wcmSettings);
                case EnterprisePropertyType.Datetime:
                    return MapDateTimePropertyValue(oldValue);
                case EnterprisePropertyType.User:
                    return MapUserPropertyValue(oldValue, Identities);
                case EnterprisePropertyType.Taxonomy:
                    return MapTaxonomyPropertyValue(oldValue);
                case EnterprisePropertyType.Boolean:
                    return MapBooleanPropertyValue(oldValue);
                case EnterprisePropertyType.Object:
                    return MapObjectPropertyValue(oldValue);
                default:
                    return null;
            }
        }

        public static JToken MapImagePropertyValue(JToken oldValue, WCMContextSettings wcmSettings)
        {
            var imageSrc = HtmlParser.ParseImageSrc(oldValue.ToString());
            imageSrc = UrlHelper.MapUrl(imageSrc, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings);

            if (string.IsNullOrEmpty(imageSrc))
                return null;
            try
            {//Diem - 03Aug2022: handle video in page image
                var tempString = HttpUtility.UrlDecode(imageSrc);
                int index = tempString.IndexOf("{");
                string piece = tempString.Substring(index);
                var videoData = JsonConvert.DeserializeObject<VideoPropertyValue>(piece);

                if (videoData.isVideo == true)
                {
                    return JToken.FromObject(videoData.configuration);
                }
                else
                {
                    return JToken.FromObject(EnterprisePropertyFactory.CreateDefaultMediaPropertyValue(imageSrc));
                }
            }
           catch(Exception ex)
            {
                return JToken.FromObject(EnterprisePropertyFactory.CreateDefaultMediaPropertyValue(imageSrc));
            }
        }
        public static identity GetUserIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        { 
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return new identity() { id = item.Id, type=item.Type};
                }
            }
            return null;
        }

        public static JToken MapUserPropertyValue(JToken oldValue, ItemQueryResult<IResolvedIdentity> Identities)
        { 
            var users = oldValue.ToObject<List<string>>().Where(x => !string.IsNullOrEmpty(x)).ToList();
            //hieu rem
            //return JToken.FromObject(users.Select(user => new UserPropertyValue { uid = user }).ToList());
            var arr = users.Select(user => GetUserIdentitybyEmail(Identities, user)).ToList();
            return JToken.FromObject(arr.ToArray());
        }

        public static JToken MapUserPropertyValue(FieldUserValue oldValue, ItemQueryResult<IResolvedIdentity> Identities)
        {
            var users = new List<string> { oldValue.Email };
            //hieu rem
            //return JToken.FromObject(users.Select(user => new UserPropertyValue { uid = user }).ToList());
            return JToken.FromObject(users.Select(user => GetUserIdentitybyEmail(Identities, user)).ToList());
        }

        public static JToken MapTaxonomyPropertyValue(JToken oldValue)
        {
            var terms = oldValue.ToObject<List<G1TaxonomyPropertyValue>>();
            //Console.WriteLine(oldValue);
            return JToken.FromObject(terms.Select(term => term.TermGuid.ToString()).ToList());
        }

        public static JToken MapBooleanPropertyValue(JToken oldValue)
        {
            var value = bool.Parse(oldValue.ToString());
            return JToken.FromObject(value);
        }

        public static JToken MapObjectPropertyValue(JToken oldValue)
        {
            var value = JObject.Parse(oldValue.ToString());
            return JToken.FromObject(value);
        }

        public static JToken MapTextPropertyValue(JToken oldValue, WCMContextSettings wcmSettings)
        {
            var textValue = oldValue.ToString();
            var urls = HtmlParser
                .ParseAllUrl(textValue)
                .Distinct()
                .OrderByDescending(x => x.Length)
                .ToList();

            foreach (var url in urls)
            {
                if(url != "/")
                {
                    var newUrl = UrlHelper.MapUrl(url, wcmSettings.SharePointUrl, wcmSettings.SharePointLocationMappings);

                    var urlWithHref = "href=\"" + url;
                    var urlWithSrc = "src=\"" + url;
                    var urlWithDataMCEHref = "data-mce-href=\"" + url;

                    var newUrlWithHref = "href=\"" + newUrl;
                    var newWithSrc = "src=\"" + newUrl;
                    var newWithDataMCEHref = "data-mce-href=\"" + newUrl;

                    textValue = textValue.Replace(urlWithHref, newUrlWithHref);
                    textValue = textValue.Replace(urlWithSrc, newWithSrc);
                    textValue = textValue.Replace(urlWithDataMCEHref, newWithDataMCEHref);

                    //Diem - 24Aug2022: replace innerText of <a> tag if it contain URL same as href
                    if (!UrlHelper.IsAbsoluteUrl(url) && !UrlHelper.ValidateUrl(url))
                    {
                        var oldUrl = UrlCombine.Combine(UrlHelper.GetAuthority(wcmSettings.SharePointUrl), url);
                        oldUrl = Uri.UnescapeDataString(oldUrl);
                        if (UrlHelper.IsImageUrl(oldUrl))
                            continue;

                        if(textValue.Contains(oldUrl))
                        {
                            textValue = textValue.Replace(oldUrl, newUrl);
                        }
                    }

                }
            }

            return JToken.FromObject(textValue);
        }

        public static JToken MapDateTimePropertyValue(JToken oldValue)
        {
            return oldValue;
        }
    }
}

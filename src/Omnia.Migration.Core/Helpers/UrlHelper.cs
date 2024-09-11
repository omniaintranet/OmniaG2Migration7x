using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UrlCombineLib;

namespace Omnia.Migration.Core.Helpers
{
    public static class UrlHelper
    {
        public static bool IsAbsoluteUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri result);
        }

        public static string MapAllUrlsInText(string text, string spUrl, Dictionary<string, string> urlsMap)
        {
            var newText = text;
            var urls = HtmlParser
                      .ParseAllUrl(newText)
                      .Distinct()
                      .OrderByDescending(x => x.Length)
                      .ToList();

            foreach (var url in urls)
            {
                var newUrl = UrlHelper.MapUrl(url, spUrl, urlsMap);
                newText = newText.Replace(url, newUrl);
            }

            return newText;
        }

        public static string MapUrl(string url, string spUrl, Dictionary<string, string> urlsMap, bool isLink = false)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            url = HttpUtility.HtmlDecode(url);

            //08082022 -Diem: do not combine root SP if it's a shared links/my links, even it's not a valid URL
            if (!IsAbsoluteUrl(url) && !ValidateUrl(url) && isLink==false)
            {                
                url = UrlCombine.Combine(GetAuthority(spUrl), url);
                
                if (IsImageUrl(url))
                    return url;
                if (IsDocumentUrl(url))
                    return url;
            }
                
            List<string> srcUrls = urlsMap.Keys.OrderByDescending(x => x.Length).ToList();

            foreach (var srcUrl in srcUrls)
            {
                var lowerUrl = url.ToLower();
                if (lowerUrl.StartsWith(srcUrl.ToLower()))
                {
                    url = lowerUrl.Replace(srcUrl.ToLower(), urlsMap[srcUrl].ToLower());

                    // remove .aspx from https://contoso.sharepoint.com/#/nyheter/sidor/article.aspx 
                    if (srcUrl.Contains("#") && url.Contains(".aspx"))
                        url = url.Substring(0, url.IndexOf(".aspx"));

                    if (url.EndsWith("/newsstartpage"))
                        url = url.Substring(0, url.Length - "/newsstartpage".Length);

                    break;
                }
            }

            return url;
        }

        public static bool IsImageUrl(string url)
        {
            return url.ToLower().Contains(".png") ||
                   url.ToLower().Contains(".jpg") ||
                   url.ToLower().Contains(".jpeg") ||
                   url.ToLower().Contains(".image-jpeg") ||
                   url.ToLower().Contains(".image-png") ||
                   url.ToLower().Contains(".svg") ||
                   url.ToLower().Contains(".gif");
        }
        public static bool IsDocumentUrl(string url)
        {
            return url.ToLower().Contains(".doc") ||
                   url.ToLower().Contains(".docx") ||
                   url.ToLower().Contains(".xls") ||
                   url.ToLower().Contains(".xlsx") ||
                   url.ToLower().Contains(".pdf") ||
                   url.ToLower().Contains(".ppt") ||
                   url.ToLower().Contains(".pptx")||
                   url.ToLower().Contains(".odt") ||
                   url.ToLower().Contains(".ods") ||
                   url.ToLower().Contains(".txt") ||
                   url.ToLower().Contains(".zip");
        }
        public static string GetAuthority(string url)
        {
            return new Uri(url).GetLeftPart(UriPartial.Authority);
        }

        public static string GetRelativeUrl(string url)
        {
            if (IsAbsoluteUrl(url))
            {
                url = url.Replace(GetAuthority(url), string.Empty);
            }

            return url;
        }
        public static bool ValidateUrl(string value)
        {
            value = value.Trim();
            if (value == "") return true;
           

            Regex pattern = new Regex(@"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$");
            Match match = pattern.Match(value);
            if (match.Success == false) return false;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Omnia.Migration.Core.Helpers
{
    public static class HtmlParser
    {
        public static string ParseImageSrc(string html)
        {
            return Regex.Match(html, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
        }

        public static List<string> ParseAllImageSrcs(string html)
        {
            return Regex.Matches(html, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Select(x => x.Groups[1].Value).ToList();
        }

        public static List<string> ParseAllHref(string html)
        {
            return Regex.Matches(html, "<a.+?href=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Select(x => x.Groups[1].Value).ToList();
        }

        public static List<string> ParseAllImageUrls(string text)
        {
            //return Regex.Matches(text, "(http)?s?:?(\\/\\/[^\"']*\\.(?:png|jpg|jpeg|gif|png|svg|bmp|webp))(\\?(\\S*\\=\\S*)(\\&\\S*\\=\\S*)*)?(?<![\"])", RegexOptions.IgnoreCase).Select(x => x.Value).ToList();
            return Regex.Matches(text, "(http)?s?:?(\\/\\/[^\"',]*\\.(?:png|jpg|jpeg|gif|png|svg|bmp|webp))", RegexOptions.IgnoreCase).Select(x => x.Value).ToList();
        }

        public static List<string> ParseAllUrls(string text)
        {
            return Regex.Matches(text, "(https?):\\/\\/(www\\.)?[a-z0-9\\.:].*?(?=\\s)", RegexOptions.IgnoreCase).Select(x => x.Value).ToList();
        }

        public static List<string> ParseAllUrl(string html)
        {
            List<string> result = new List<string>();
            result.AddRange(ParseAllImageSrcs(html));
            result.AddRange(ParseAllHref(html));

            return result;
        }
        public static List<string> ParseAllDocumentUrls(string text)
        {            
            return Regex.Matches(text, "(http)?s?:?(\\/\\/[^\"',]*\\.(?:doc|docx|pdf|xls|xlsx|ppt|pptx|ods|odt|txt))(\\?(\\S*\\=\\S*)(\\&\\S*\\=\\S*)*)?(?<![\",])", RegexOptions.IgnoreCase).Select(x => x.Value).ToList();
        }
    }
}

using Newtonsoft.Json;
using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class RelatedLinksBlockData : BaseBlockData
    {        
        public override string GetElementName()
        {
            return "";
        }
    }

    public class RelatedLinkBlockSettings: Omnia.Fx.Models.Layouts.BlockSettings
    {
        public string backgroundColor { get; set; }

        public string borderColor { get; set; }

        public string textColor { get; set; }
    }

    public class RelatedLinksData: OmniaJsonBase
    {
        public List<RelatedLink> links { get; set; }
    }

    public class RelatedLinkIcon
    {
        public string iconType { get; set; }
    }

    public class RelatedLink
    {
        public RelatedLinkIcon icon { get; set; }

        public int index { get; set; }

        public string linkType { get; set; }

        public string title { get; set; }

        public string url { get; set; }

        public bool openInNewWindow { get; set; }
    }

    public static class RelatedLinkTypes
    {
        public const string Heading = "wcm-link-heading";

        public const string CustomLink = "wcm-custom-link";

        public const string PageLink = "wcm-link-page";
    }
}

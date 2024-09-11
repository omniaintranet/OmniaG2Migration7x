using Omnia.Fx.Models.JsonTypes;
using System;
using Omnia.WebContentManagement.Models.Variations;
using System.Collections.Generic;

namespace Omnia.Migration.Models.BlockData
{
    public class RSSBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "wcm-rssreader-block";
        }

        public RSSBlockData()
        {
            Settings = new RSSBlockSetting();
            Data = new RSSData();
        }
    }

    public class RSSBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        public Guid selectedViewId { get; set; }
        public string source { get; set; }
        public string spacing { get; set; }
        public int itemLimit { get; set; }
        public bool isOpenNewWindow { get; set; }
        public bool showActualDay { get; set; }
        public bool showTitle { get; set; }
        public RollupPagingType pagingType { get; set; }
        public VariationString title { get; set; }
        public RSSViewSettings viewSettings { get; set; }
    }
    public class RSSViewSettings
    {
        public List<string> selectProperties { get; set; }

        public RSSViewSettings()
        {
            selectProperties = new List<string>();
        }
    }
    public class RSSData : OmniaJsonBase
    {
       
    }
}

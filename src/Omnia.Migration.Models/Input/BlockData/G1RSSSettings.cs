using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1RSSSetting: G1BlockSetting
    {
        public G1RSSSettingData Settings { get; set; }
    }

    public class G1RSSSettingData
    {
        public int viewType { get; set; }     
        public bool RSSViewType { get; set; }
        public object viewPortSettings { get; set; }
        public bool isOpenNewWindow { get; set; }
        public int pageSize { get; set; }              
        public RSSTargeting targeting { get; set; }       
        public string source { get; set; }
        public string title { get; set; }
        public bool showTitle { get; set; }
        public TitleSettings titleSettings { get; set; }
        public class RSSTargeting
        {            
            public bool hasTargeting { get; set; }
            public string targetingDefinitionId { get; set; }
        }       
    }
}

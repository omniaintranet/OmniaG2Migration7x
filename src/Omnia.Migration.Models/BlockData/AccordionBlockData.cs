using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class AccordionBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "wcm-accordion";
        }

        public AccordionBlockData()
        {
            Settings = new AccordionBlockSetting();
            Data = new AccordionData();
        }
    }

    public class AccordionBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        
    }

    public class AccordionData : OmniaJsonBase
    {
        public List<AccordionDataItem> accordions { get; set; }

        public AccordionData()
        {
            accordions = new List<AccordionDataItem>();            
        }
    }

    public class AccordionDataItem
    {
        public string title { get; set; }
        public string content { get; set; }
        public int id { get; set; }
    }
}

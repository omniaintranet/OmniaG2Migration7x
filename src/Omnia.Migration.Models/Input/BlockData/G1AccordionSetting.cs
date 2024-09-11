using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1AccordionSetting: G1BlockSetting
    {
        public G1AccordionSettingData Settings { get; set; }
    }

    public class G1AccordionSettingData
    {
        public List<G1AaccordionSettingDataItem> blocks { get; set; }

        public G1AccordionSettingData()
        {
            blocks = new List<G1AaccordionSettingDataItem>();                
        }
    }

    public class G1AaccordionSettingDataItem
    {
        public string title { get; set; }

        public string content { get; set; }

        public bool visible { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class BlockLayoutItemSettings
    {
        public BlockLayoutSettingsBackground background { get; set; }

        public string bgColor { get; set; }

        public string chrome { get; set; }

        public string css { get; set; }

        public string customCssClasses { get; set; }

        public List<string> deviceBreakPoints { get; set; }

        public bool hidden { get; set; }

        public int paddingBottom { get; set; }

        public int paddingLeft { get; set; }

        public int paddingRight { get; set; }

        public int paddingTop { get; set; }

        public string titleSettings { get; set; }

        public BlockLayoutSettingsTargetingFilter targetingFilterProperties { get; set; }

        public BlockLayoutItemSettings()
        {
            deviceBreakPoints = new List<string>();
            background = new BlockLayoutSettingsBackground();
            targetingFilterProperties = new BlockLayoutSettingsTargetingFilter();
        }
    }

    public class BlockLayoutSettingsTargetingFilter
    {
        public BlockLayoutSettingsEnterpriseProperties enterprisePropertiesSettings { get; set; }

        public BlockLayoutSettingsTargetingFilter()
        {
            enterprisePropertiesSettings = new BlockLayoutSettingsEnterpriseProperties();
        }
    }

    public class BlockLayoutSettingsEnterpriseProperties
    {

    }

    public class BlockLayoutSettingsBackground
    {
        public List<string> colors { get; set; }

        public string image { get; set; }

        public int elevation { get; set; }

        public int borderWidth { get; set; }

        public BlockLayoutSettingsBackground()
        {
            colors = new List<string>();
        }
    }
}

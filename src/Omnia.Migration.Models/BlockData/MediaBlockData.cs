using Omnia.Fx.Models.JsonTypes;
using Omnia.Migration.Models.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class MediaBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "";
        }
    }

    public class MediaBlockSettings : Omnia.Fx.Models.Layouts.BlockSettings
    {
        public string pageProperty { get; set; }
    }

    public class MediaData : OmniaJsonBase
    {
        public bool image { get; set; }

        public bool video { get; set; }

        public MediaPropertyValue imageContentObj { get; set; }

        public object videoContent { get; set; } // Video is not supported yet
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Omnia.Fx.Models.JsonTypes;
using Omnia.WebContentManagement.Models.Layout;

namespace Omnia.Migration.Models.BlockData
{
    public class ContentBlockData : BaseBlockData
    {        
        public override string GetElementName()
        {
            return "";
        }
    }

    public class ContentBlockSettings: Omnia.Fx.Models.Layouts.BlockSettings
    {
        public string pageProperty { get; set; }
    }

    public class ContentData: OmniaJsonBase
    {
        public string content { get; set; }
    }
}

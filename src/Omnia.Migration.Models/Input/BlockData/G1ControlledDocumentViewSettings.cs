using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1ControlledDocumentViewSettings : G1BlockSetting
    {
        public G1ControlledDocumentViewData? Settings { get; set; }
    }

    public class G1ControlledDocumentViewData : G1BaseDocumentRollupData
    {
        public string sortByProperty { get; set; }
        public int searchScope { get; set; }
    }
}

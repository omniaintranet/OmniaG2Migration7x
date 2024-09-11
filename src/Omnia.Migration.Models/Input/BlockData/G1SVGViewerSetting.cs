using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1SVGViewerSetting: G1BlockSetting
    {
        public G1SVGViewerSettingData Settings { get; set; }
    }

    public class G1SVGViewerSettingData
    {
        public string name { get; set; }

        public string url { get; set; }

        public string tempurl { get; set; }
        public string innerHtml { get; set; }
        public bool spinner { get; set; }

        public string padding { get; set; }//sometime its value is string (ex: 48% , 0px)

        public bool showDownload { get; set; }
        public bool showDownloadHoverState { get; set; }
        public string searchquery { get; set; }
    }   
}

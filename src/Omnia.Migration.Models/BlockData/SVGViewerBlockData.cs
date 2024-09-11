using Omnia.Fx.Models.JsonTypes;
using Omnia.Migration.Models.EnterpriseProperties;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class SVGViewerBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "kun-svg-viewer";
        }

        public SVGViewerBlockData()
        {
            Settings = new SVGViewerBlockSetting();
            Data = new SVGViewerData();
        }
    }

    public class SVGViewerBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {       
        public bool enableDownloadButton { get; set; }
        //public string pageProperty { get; set; }
        public VariationString blockTitle { get; set; }
        public SVGSpacing spacing { get; set; }
        public SVGViewerData svgImage { get; set; }
    }
    public class SVGSpacing
    {
        public int bottom { get; set; }

        public int left { get; set; }

        public int right { get; set; }

        public int top { get; set; }
    }
    public class SVGViewerData : OmniaJsonBase
    {
        public string documentUrl { get; set; } //SVG image link

        public string format { get; set; }//image type (svg)

        public string name { get; set; }//image name

        //public int id { get; set; } //Image id in SP library

        //public Guid listId { get; set; } //SP list id where the image is stored

        public string spWebUrl { get; set; } //SP site URL

        //public string base64 { get; set; } //image string      
    }   
}

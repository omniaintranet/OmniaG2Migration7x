using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class ScriptHtmlBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "wcm-html-script-block";
        }

        public ScriptHtmlBlockData()
        {
            Settings = new ScriptHtmlBlockSetting();
            Data = new ScriptHtmlData();
        }
    }

    public class ScriptHtmlBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        
    }

    public class ScriptHtmlData : OmniaJsonBase
    {
        public string html { get; set; }
        public string js { get; set; }
        public string css { get; set; }
        public bool hiddenBlock { get; set; }
        public bool runInIframe { get; set; }
        public bool runScriptInEditMode { get; set; }
    }
}

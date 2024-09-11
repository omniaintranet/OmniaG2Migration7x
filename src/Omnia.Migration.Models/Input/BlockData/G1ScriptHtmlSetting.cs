using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1ScriptHtmlV1Setting: G1BlockSetting
    {
        public G1ScriptHtmlV1SettingData Settings { get; set; }
    }

    public class G1ScriptHtmlV1SettingData
    {
        public string content { get; set; }
    }

    public class G1ScriptHtmlV2Setting : G1BlockSetting
    {
        public G1ScriptHtmlV2SettingData Settings { get; set; }
    }

    public class G1ScriptHtmlV2SettingData
    {
        public object settings { get; set; }

        public G1ScriptHtmlV2SettingSubData data { get; set; }

        public G1ScriptHtmlV2SettingData()
        {
        }
    }

    public class G1ScriptHtmlV2SettingSubData
    {
        public string html { get; set; }
        public string js { get; set; }
        public string css { get; set; }
        public bool hiddenBlock { get; set; }
        public bool runInIframe { get; set; }
        public bool runScriptInEditMode { get; set; }
    }
}

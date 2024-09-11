using Omnia.Migration.Models.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1BannerSetting : G1BlockSetting
    {
        public G1BannerSettingData Settings { get; set; }
    }


    public class G1BannerSettingData
    {
        public string Title { get; set; }

        public TitleSettings TitleSettings { get; set; }

        public string ImageUrl { get; set; }

        public string Content { get; set; }

        public string LinkUrl { get; set; }

        public string Footer { get; set; }

        public string TitleColor { get; set; }

        public string ContentColor { get; set; }

        public string BackgroundColor { get; set; }

        public string ViewId { get; set; }

        public int? BannerType { get; set; }

        public bool IsOpenLinkNewWindow { get; set; }
        public VideoPropertyValue mediaData { get; set; }
    }
}

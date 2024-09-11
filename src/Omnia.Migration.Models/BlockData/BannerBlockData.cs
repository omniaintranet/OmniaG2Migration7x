using Omnia.Fx.Models.JsonTypes;
using Omnia.Migration.Models.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class BannerBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "wcm-banner-block";
        }

        public BannerBlockData()
        {
            Settings = new BannerBlockSetting();
            Data = new BannerData();
        }
    }

    public class BannerBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {

    }

    public class BannerData : OmniaJsonBase
    {
        public string title { get; set; }

        public string content { get; set; }

        public string footer { get; set; }

        public string imagesrc { get; set; }

        public string videosrc { get; set; }

        public string imagesvg { get; set; }

        public int layout { get; set; }

        public BannerSpacing spacing { get; set; }

        public BannerColorData color { get; set; }

        public BannerLinkSetting linkSetting { get; set; }

        public BannerMediaData mediaContent { get; set; }
    }

    public class BannerSpacing
    {
        public int top { get; set; }

        public int left { get; set; }

        public int right { get; set; }

        public int bottom { get; set; }
    }

    public class BannerColorData
    {
        public string backgroundColor { get; set; }

        public string contentColor { get; set; }

        public string footerColor { get; set; }

        public string titleColor { get; set; }
    }

    public class BannerLinkSetting
    {
        public BannerLinkData link { get; set; }
    }

    public class BannerLinkData
    {
        public string icon { get; set; }

        public int linkType { get; set; }

        public string title { get; set; }

        public string url { get; set; }

        public bool openInNewWindow { get; set; }
    }

    public class BannerMediaData
    {
        public BannerImageData imageContent { get; set; }
    }

    public class BannerImageData
    {
        public MediaConfiguration configuration { get; set; }

        public List<Ratio> extraRatios { get; set; }

        public string imageSrc { get; set; }
    }
    public class BannerVideoSettings : OmniaJsonBase
    {
        public string title { get; set; }

        public string content { get; set; }

        public string footer { get; set; }      

        public int layout { get; set; }

        public BannerSpacing spacing { get; set; }

        public BannerColorData color { get; set; }

        public BannerLinkSetting linkSetting { get; set; }

        public BannerVideoData mediaContent { get; set; }
    }
    public class BannerVideoData
    {
        public int omniaMediaType = 1;
        public string html { get; set; }
        public string videoUrl { get; set; }
        public bool mute { get; set; }
        public bool autoPlay { get; set; }
        public string thumbnailUrl { get; set; }
    }
}

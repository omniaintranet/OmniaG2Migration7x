using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.EnterpriseProperties
{
    public class MediaPropertyValue
    {
        public MediaConfiguration configuration { get; set; }

        public List<Ratio> ratios { get; set; }

        public string src { get; set; }
    }

    public class Ratio
    {
        public string ratioDisplayName { get; set; }

        public int xRatio { get; set; }

        public int yRatio { get; set; }

        public MediaCropArea ratioCropArea { get; set; }
    }

    public class MediaConfiguration
    {
        public MediaSize size { get; set; }

        public MediaCropRatio cropRatio { get; set; }

        public MediaCropArea cropArea { get; set; }
    }

    public class MediaSize
    {
        public string width { get; set; }

        public string height { get; set; }

        public static MediaSize Default = new MediaSize { height = "100%", width = "100%" };
    }

    public class MediaCropArea
    {
        public double width { get; set; }

        public double height { get; set; }

        public double x { get; set; }

        public double y { get; set; }

        public static MediaCropArea Default = new MediaCropArea
        {
            height = 1,
            width = 1,
            x = 0,
            y = 0
        };
    }

    public class MediaCropRatio
    {
        public double x { get; set; }

        public double y { get; set; }
    }

    public class VideoPropertyValue
    {
        public string mediaUrl { get; set; }
        public bool isVideo { get; set; }
        public VideoConfiguration configuration { get; set; }
        public class VideoConfiguration
        {
            public int omniaMediaType = 1;
            public string html { get; set; }
            public string videoUrl { get; set; }
            public bool mute { get; set; }
            public bool autoPlay { get; set; }
            public string thumbnailUrl { get; set; }
        }
    }
}

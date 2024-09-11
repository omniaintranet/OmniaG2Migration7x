using Omnia.Migration.Models.BlockData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Factories
{
    public static class BlockDataFactory
    {
        public static ContentBlockData CreateContentBlockData(string propName, string content)
        {
            return new ContentBlockData
            {
                Settings = new ContentBlockSettings { pageProperty = propName },
                Data = new ContentData { content = content }
            };
        }

        public static MediaBlockData CreateMediaBlockData(string propName, bool isImage, bool isVideo)
        {
            return new MediaBlockData
            {
                Settings = new MediaBlockSettings { pageProperty = propName },
                Data = new MediaData { image = isImage, video = isVideo }
            };
        }

        public static RelatedLinksBlockData CreateRelatedLinksBlockData(List<RelatedLink> links)
        {
            return new RelatedLinksBlockData
            {
                Settings = new RelatedLinkBlockSettings { backgroundColor = "", borderColor = "", textColor = "" }, 
                Data = new RelatedLinksData { links = links }
            };
        }
        //public static SVGViewerBlockData CreateSVGViewerBlockData(SVGViewerData svgData, string propName)
        //{
        //    return new SVGViewerBlockData
        //    {
        //        Settings = new SVGViewerBlockSetting { pageProperty=propName },
        //        Data = new SVGViewerData {
        //            documentUrl=svgData.documentUrl,
        //            format=svgData.format
        //        }
        //    };
        //}
    }
}

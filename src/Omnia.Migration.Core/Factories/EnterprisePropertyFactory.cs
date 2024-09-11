using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Migration.Models.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Omnia.Migration.Core.Factories
{
    public static class EnterprisePropertyFactory
    {
        public static MediaPropertyValue CreateDefaultMediaPropertyValue(string imageSrc = "", int customXRatio = 16, int customYRatio = 9)
        {
            return new MediaPropertyValue
            {
                src = imageSrc,
                ratios = new List<Ratio> {
                    new Ratio { ratioDisplayName = "Landscape", xRatio = 16, yRatio = 9 },
                    new Ratio { ratioDisplayName = "Square", xRatio = 1, yRatio = 1 },
                    new Ratio { ratioDisplayName = "Portrait", xRatio = 2, yRatio = 3 },
                    new Ratio { ratioDisplayName = "Custom", xRatio = customXRatio, yRatio = customYRatio, ratioCropArea = MediaCropArea.Default },
                },
                configuration = new MediaConfiguration
                {
                    cropArea = MediaCropArea.Default,
                    cropRatio = new MediaCropRatio { x = customXRatio, y = customYRatio },
                    size = MediaSize.Default
                }
            };
        }
    }
}

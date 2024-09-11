using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Mappings
{
    public class LayoutMapping
    {
        public int LayoutId { get; set; }

        public Dictionary<string, string> ZoneMappings { get; set; }        

        public Guid? PageImageBlock { get; set; }

        public Guid? RelatedLinksBlock { get; set; }

        public Guid? MainContentBlock { get; set; }

        public Guid? AccordionBlock { get; set; }

        public bool UseAutoMapping { get; set; }
        public Guid? SVGViewerBlock { get; set; }
        public LayoutMapping()
        {
        }
    }
}

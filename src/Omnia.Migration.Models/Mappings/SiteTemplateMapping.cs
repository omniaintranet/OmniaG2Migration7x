using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Mappings
{
    public class SiteTemplateMapping
    {
        public Guid G1TemplateId { get; set; }
        public Guid G2TemplateId { get; set; }

        public Guid BusinessProfileId { get; set; }
        public Dictionary<string, EnterprisePropertyMapping> Properties { get; set; }

        public SiteTemplateMapping()
        {
            Properties = new Dictionary<string, EnterprisePropertyMapping>();
        }
    }
}

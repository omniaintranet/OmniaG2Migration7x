using Omnia.Migration.Models.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class LookupItem
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class WCMContextSettings
    {
        public string Language { get; set; }

        public int CultureInfo { get; set; }

        public string SharePointUrl { get; set; }

        public Guid PublishingAppId { get; set; }

        public int DefaultVariationId { get; set; }

        public int? PageCollectionId { get; set; }

        public Dictionary<string, LayoutMapping> LayoutMappings { get; set; }

        public Dictionary<string, EnterprisePropertyMapping> EnterprisePropertiesMappings { get; set; }        

        public Dictionary<string, int> VariationMappings { get; set; }

        public List<LookupItem> SharePointLocations { get; set; }

        public List<SearchPropertyMapping> SearchProperties { get; set; }

        public List<NewsCenterMapping> NewsCenterMappings { get; set; }

        public List<SiteTemplateMapping> SiteTemplateMappings { get; set; }

        public Dictionary<string, string> SharePointLocationMappings {
            get
            {
                return SharePointLocations.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public string DefaultPeopleNameProperty { get; set; }

        public string DefaultRelatedLinksProperty { get; set; }
        public string DefaultSVGViewerProperty { get; set; }
        public string DefaultAccordionProperty { get; set; }

        public string DatabaseConnectionString { get; set; }

        public WCMContextSettings()
        {
            LayoutMappings = new Dictionary<string, LayoutMapping>();
            SharePointLocations = new List<LookupItem>();
            EnterprisePropertiesMappings = new Dictionary<string, EnterprisePropertyMapping>();
            SearchProperties = new List<SearchPropertyMapping>();
            NewsCenterMappings = new List<NewsCenterMapping>();
            SiteTemplateMappings = new List<SiteTemplateMapping>();
            VariationMappings = new Dictionary<string, int>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1NewsViewerSetting
    {
        public G1NewsViewerSettingData Settings { get; set; }
    }

    public class G1NewsViewerSettingData
    {
        public string view { get; set; }

        public G1NewsViewerQuery query { get; set; }
    }

    public class G1NewsViewerQuery
    {
        public List<G1NewsViewerNewsCenterQuery> newsCenterQuery { get; set; }

        public int itemLimit { get; set; }

        public G1NewsViewerQuery()
        {
            newsCenterQuery = new List<G1NewsViewerNewsCenterQuery>();
        }
    }

    public class G1NewsViewerNewsCenterQuery
    {
        public string newsCenterUrl { get; set; }
        public bool useTargetingSettings { get; set; }
        public List<G1NewsViewerNewsCenterQueryFilter> filters { get; set; }

        public G1NewsViewerNewsCenterQuery()
        {
            filters = new List<G1NewsViewerNewsCenterQueryFilter>();
        }
    }

    public class G1NewsViewerNewsCenterQueryFilter
    {
        public string fieldName { get; set; }
        public int filterType { get; set; }
        public string typeAsString { get; set; }
        public bool includeChildTerms { get; set; }
        public bool includeEmpty { get; set; }
        public string termSetId { get; set; }
        public object value { get; set; }
        public List<string> taxonomyValues { get; set; }

        public List<G1NewsViewerTaxonomyValue> taxonomyValue { get; set; }
    }

    public class G1NewsViewerTaxonomyValue
    {
        public string Id { get; set; }
    }
}

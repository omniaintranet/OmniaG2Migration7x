using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class PageRollupBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "owcm-page-rollup";
        }

        public PageRollupBlockData()
        {
            Settings = new PageRollupBlockSetting();
            Data = new PageRollupData();
        }
    }

    public class PageRollupBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        public List<PageRollupResource> resources { get; set; }

        public bool trimDuplicates { get; set; }
        public bool lastModifiedByCurrentUser { get; set; }

        public string sortBy { get; set; }
        public bool sortDescending { get; set; }

        public RollupPagingType pagingType { get; set; }
        public string itemLimit { get; set; }

        public int scope { get; set; }

        public Guid selectedViewId { get; set; }
        public PageRollupViewSettings viewSettings { get; set; }


        public PageRollupBlockSetting()
        {
            resources = new List<PageRollupResource>();
            viewSettings = new PageRollupViewSettings();
        }
    }

    public class PageRollupData : OmniaJsonBase
    { }

    public class PageRollupResource
    {
        public string id { get; set; }

        public List<object> filters { get; set; }

        public PageRollupResource()
        {
            filters = new List<object>(); // We don't migrate page rollup filters for now
        }
    }

    public class PageRollupFilter
    {
        public string property { get; set; }
        public int type { get; set; }
        public object valueObj { get; set; }
    }

    public class PageRollupFilterTaxonomyValue
    {
        public int filterType { get; set; }
        public bool includeChildTerms { get; set; }
        public bool includeEmpty { get; set; }
        public List<string> fixedTermIds { get; set; }

        public PageRollupFilterTaxonomyValue()
        {
            fixedTermIds = new List<string>();
        }
    }

    public class PageRollupFilterBooleanValue
    {
        public bool value { get; set; }
    }

    public class PageRollupViewSettings
    {
        public bool showContentAsDialog { get; set; }
        public Dictionary<string, string> pageDialogPropsMapping { get; set; }
        public List<string> selectProperties { get; set; }

        public PageRollupViewSettings()
        {
            pageDialogPropsMapping = new Dictionary<string, string>();
            selectProperties = new List<string>();
        }
    }

    public class PageRollupListViewSettings: PageRollupViewSettings
    {
        public List<RollupListViewSettingsColumn> columns { get; set; }

        public PageRollupListViewSettings() : base()
        {
            columns = new List<RollupListViewSettingsColumn>();
        }
    }

    public class PageRollupListWithImageViewSettings : PageRollupViewSettings
    {
        public string summaryProp { get; set; }
        public string imageProp { get; set; }
        public string dateProp { get; set; }

        public PageRollupListWithImageViewSettings() : base()
        {
        }
    }
}

using Newtonsoft.Json;
using Omnia.Fx.Models.JsonTypes;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class DocumentRollupBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "odm-document-rollup";
        }

        public DocumentRollupBlockData()
        {
            Settings = new DocumentRollupBlockSetting();
            Data = new DocumentRollupData();
        }
    }

    public class DocumentRollupBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        public VariationString title { get; set; }
        public bool openInClientApp { get; set; }
        public bool trimByFollowingSites { get; set; }
        public bool lastModifiedByCurrentUser { get; set; }

        public string sortby { get; set; }//Diem - 31102022: replace "sortBy"
        public bool sortDescending { get; set; }

        public RollupPagingType pagingType { get; set; }
        public int itemLimit { get; set; }

        public string query { get; set; }
        public DocumentRollupQueryScope searchScope { get; set; }

        public Guid selectedViewId { get; set; }
        public DocumentRollupViewSettings viewSettings { get; set; }

        public List<RollupFilter> filters { get; set; }
        public RollupRefinerPositions filterPosition { get; set; }

        public List<RollupRefiner> refiners { get; set; }
        public RollupRefinerPositions refinerPosition { get; set; }

        public string dayLimitProperty { get; set; }
        public RollupDatePeriod dayLimitPeriod { get; set; }

        public DocumentRollupBlockSetting()
        {
            title = new VariationString();            
            viewSettings = new DocumentRollupViewSettings();
            filters = new List<RollupFilter>();
            refiners = new List<RollupRefiner>();
        }
    }

    public class DocumentRollupData : OmniaJsonBase
    { }    

    public class DocumentRollupViewSettings
    {
        public List<string> selectProperties { get; set; }

        public DocumentRollupViewSettings()
        {
            selectProperties = new List<string>();
        }
    }

    public class DocumentRollupListViewSettings : DocumentRollupViewSettings
    {
        public List<RollupListViewSettingsColumn> columns { get; set; }

        public DocumentRollupListViewSettings(): base()
        {
            columns = new List<RollupListViewSettingsColumn>();
        }
    }

    public enum DocumentRollupQueryScope
    {
        AllDocuments = 0,
        PublishedDocuments = 1,
        ArchivedDocuments = 2
    }

    
}

using AngleSharp.Html;
using Newtonsoft.Json;
using Omnia.Fx.Models.EnterpriseProperties;
using Omnia.Fx.Models.JsonTypes;
using Omnia.Fx.Models.Users;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public class PeopleRollupBlockData : BaseBlockData
    {
        public override string GetElementName()
        {
            return "pan-people-rollup";
        }

        public PeopleRollupBlockData()
        {
            Settings = new PeopleRollupBlockSetting();
            Data = new PeopleRollupData();
        }
    }

    public class PeopleRollupBlockSetting : Omnia.Fx.Models.Layouts.BlockSettings
    {
        public VariationString title { get; set; }

        public int totalColumns { get; set; }
        public int itemLimit { get; set; }
        public string sortby { get; set; } //Diem - 31102022: replace "sortBy" by "sortby"
        public bool sortDescending { get; set; }

        public bool showSearchBox { get; set; }

        public List<RollupFilter> filters { get; set; }
        public RollupRefinerPositions filterPosition { get; set; }

        public List<RollupRefiner> refiners { get; set; }
        //public RollupRefinerPositions refinerPosition { get; set; }
        public int refinerPosition { get; set; }

        public object query { get; set; }
        public PeopleRollupQueryType queryType { get; set; }

        public PeopleRollupViewSettings viewSettings { get; set; }
        public RollupPagingType pagingType { get; set; }

        public Guid selectedViewId { get; set; }            

       
       
     
        public bool openProfileInNewTab { get; set; }
        public bool hideFilterByDefault { get; set; }
       
       

    
     

       
   
   

        public PeopleRollupBlockSetting()
        {
            title = new VariationString();
           filters = new List<RollupFilter>();
           refiners = new List<RollupRefiner>();
        }
    }
    public class G2Spacing
    {
        public int top { get; set; }

        public int left { get; set; }

        public int right { get; set; }

        public int bottom { get; set; }
    }
    public class birthday
    {
        public int birthdayPeriod { get; set; }
        public string birthdayProperty { get; set; }
    }
    public class breakPointSettings
    {
        public string id { get; set; }
    }
    public class G2Title
    {
        public bool isMultilingualString { get; set; }
    }
    public class PeopleRollupData: OmniaJsonBase
    {
                        
    }
    

    public class PeopleRollupViewSettings
    {
        public List<string> selectProperties { get; set; }

        public PeopleRollupViewSettings()
        {
            selectProperties = new List<string>();
        }
    }

    public class PeopleRollupCardViewSettings : PeopleRollupViewSettings
    {
        public bool showProfileImage { get; set; }
        public int avatarSize { get; set; }
        public List<PeopleRollupCardViewSettingsColumn> columns { get; set; }
        public string name { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

        //public int borderRadius { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int borderWidth { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public int elevation { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public string borderColor { get; set; }
        ////June 2023
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? totalColumns { get; set; }
        public PeopleRollupCardViewSettings()
        {
            columns = new List<PeopleRollupCardViewSettingsColumn>();
        }

        

    }

    public class PeopleRollupCardViewSettingsColumn
    {
        public string internalName { get; set; }
        public int type { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string themeColor { get; set; }

    }

    public enum PeopleRollupQueryType
    {
        ProfileQuery = 0,
        SharePointGroups = 1,
        ActivityQuery = 2,
        PageQuery = 3
    }

}

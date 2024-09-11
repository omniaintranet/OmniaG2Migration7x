using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1PeopleRollupSetting: G1BlockSetting
    {
        public G1PeopleRollupSettingData Settings { get; set; }
    }

    public class G1PeopleRollupSettingData
    {
        public string queryType { get; set; }
        public string title { get; set; }
        public int pageSize { get; set; }
        public bool showProfileImage { get; set; }
        public object queryData { get; set; }
        public string pagingType { get; set; }
        public int pageLinkSize { get; set; }
        public string sortByProperty { get; set; }
        public int sortByDirection { get; set; }
        public int totalColumns { get; set; }
        public int? refinerLocation { get; set; }
        public bool? showSearchBox { get; set; }
        public int refinerLayout { get; set; }
        public List<SelectedProperties> selectedProperties { get; set; }
        public TitleSettings titleSettings { get; set; }
        public List<G1SearchProperty> refiners { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int borderRadius { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int borderWidth { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int elevation { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string borderColor { get; set; }

        public G1PeopleRollupSettingData()
        {
            refiners = new List<G1SearchProperty>();
            selectedProperties = new List<SelectedProperties>();
        }
        

        public class SelectedProperties
        {
            public Guid id { get; set; }
            public bool showLabel { get; set; }
            public string themeColor { get; set; }
        }
        public class Refiners
        {
            public Guid id { get; set; }
            public int refinerLimit { get; set; }
            public int refinerOrderBy { get; set; }

        }
        public class G21Spacing
        {
            public int top { get; set; }

            public int left { get; set; }

            public int right { get; set; }

            public int bottom { get; set; }
        }
    }
}

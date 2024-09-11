using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Omnia.Migration.Models.BlockData
{
    public class RollupFilter
    {
        public string property { get; set; }

        public object valueObj { get; set; }

        public RollupFilter()
        {
            // Default value for valueObj. Don't need to do anything else
            valueObj = JsonConvert.DeserializeObject("{ \"properties\": [] }");
        }
    }

    public class RollupListViewSettingsColumn
    {
        public string internalName { get; set; }

        public bool isShowHeading { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? type { get; set; }//Thoan


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? mode { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? width { get; set; }

       
    }

    public class RollupRefiner
    {
        public int refinerOrderBy { get; set; }

        public int refinerLimit { get; set; }

        public string property { get; set; }
    }

    public enum RollupRefinerPositions
    {
        Top = 1,
        Left = 2,
        Right = 3
    }

    public enum RollupPagingType
    {
        NoPaging = 1,
        Classic = 2,
        Scroll = 3
    }

    public enum RollupManagedPropertyOption
    {
        Refinable = 1,
        Retrievable = 2,
        Sortable = 3,
        Queryable = 4
    }

    public enum RollupDatePeriod
    {
        OneWeekFromToday = 1,
        TwoWeeksFromToday = 2,
        OneMonthFromToday = 3
    }

    public enum RollupOrderDirection
    {
        Ascending = 1,
        Descending = 2
    }
}

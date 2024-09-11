using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1DocumentRollupSetting : G1BlockSetting
    {
        public G1DocumentRollupData Settings { get; set; }
    }

    public class G1BaseDocumentRollupData
    {
        public string title { get; set; }
        public int sortByDirection { get; set; }
        public int? refinerLocation { get; set; }
        public int pageSize { get; set; }
        public bool? showSearchBox { get; set; }
        public bool isOpenInOffice { get; set; }
        public bool isOpenLinkInNewWindow { get; set; }
        public string queryText { get; set; }
        public bool isInitValue { get; set; }
        public TitleSettings titleSettings { get; set; }
        public string textColor { get; set; }
        public string bgColor { get; set; }
        public string borderColor { get; set; }
        public int pagingStyle { get; set; }
        public int filterLocation { get; set; }


        public List<G1SearchProperty> columns { get; set; }
        public List<G1SearchProperty> refiners { get; set; }

        public G1BaseDocumentRollupData()
        {
            refiners = new List<G1SearchProperty>();
            columns = new List<G1SearchProperty>();
        }
    }

    public class G1DocumentRollupData: G1BaseDocumentRollupData 
    {
        public G1SearchProperty sortByProperty { get; set; }
        public int searchScope { get; set; }
    }
}

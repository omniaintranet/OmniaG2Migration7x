using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1SearchProperty
    {
        public Guid? id { get; set; }
        public string displayName { get; set; }
        public string managedProperty { get; set; }
        public string managedRetrieveProperty { get; set; }
        public string managedRefinerProperty { get; set; }
        public string managedQueryProperty { get; set; }
        public string managedSortProperty { get; set; }
        public bool? isTitleProperty { get; set; }
        public string tenantId { get; set; }
        public bool? retrievable { get; set; }
        public bool? refinable { get; set; }
        public bool? queryable { get; set; }
        public bool? sortable { get; set; }
        public int formatting { get; set; }
        public int category { get; set; }
        public string displayNameInText { get; set; }
        public int? refinerLimit { get; set; }
        public int? refinerOrderBy { get; set; }
        public string widthType { get; set; }
        public bool? isShowColumn { get; set; }
        public bool? isShowRefiner { get; set; }
    }
}

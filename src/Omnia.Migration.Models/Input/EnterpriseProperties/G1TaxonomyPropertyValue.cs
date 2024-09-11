using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.EnterpriseProperties
{
    public class G1TaxonomyPropertyValue
    {
        public string TermSetId { get; set; }
        public string TermGuid { get; set; }
        public string Label { get; set; }
        public int? WssId { get; set; }
    }
}

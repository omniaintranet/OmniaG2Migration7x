using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Mappings
{
    public enum PropertyIndexedType
    {
       
        Text = 1,
      
        Number=2,
      
        DateTime=3,
       
        Boolean=4,
        
        Person=5,
        
        Taxonomy=6,
        
        EnterpriseKeywords=7,
        
        Media=8,
        
        RichText=9,
        
        Data=10,
        
        Language=11,
       
        Tags=12,
       
        ExtendedProperty=13
    }


    public class SearchPropertyMapping
    {
        public string G1PropertyName { get; set; }

        public Guid G1PropertyId { get; set; }

        public string G2PropertyName { get; set; }

        public string ManagedPropertyName { get; set; }
        public int  type { get; set; }


    }
}

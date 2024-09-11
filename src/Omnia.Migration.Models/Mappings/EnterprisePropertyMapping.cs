using Omnia.Migration.Models.EnterpriseProperties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Mappings
{
    public class EnterprisePropertyMapping
    {
        public string PropertyName { get; set; }

        public EnterprisePropertyType PropertyType { get; set; }
    }
}

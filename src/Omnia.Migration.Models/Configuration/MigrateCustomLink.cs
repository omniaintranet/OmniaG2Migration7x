using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.Configuration
{
    public class MigrateCustomLink
    {       
        public string Cookie { get; set; }
        public string Accept { get; set; }
        public Boolean MigrateCustomLinktoG2 { get; set; }
    }
}

using Omnia.Fx.Models.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{
    // This class is no longer used in WCM 6.0. We use this as temporary fix while relying on WCM to migrate the layout to new model.
    public class LayoutData: LayoutItem
    {
        public bool Cleaned { get; set; }
      
                
    }
}

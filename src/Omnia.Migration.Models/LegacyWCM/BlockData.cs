using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{
    public class BlockData
    {
        public Omnia.Fx.Models.Layouts.BlockSettings Settings { get; set; }
        public Omnia.Fx.Models.JsonTypes.OmniaJsonBase Data { get; set; }
    }
}

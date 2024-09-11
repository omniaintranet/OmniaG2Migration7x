using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{
    public class LayoutItem: Omnia.Fx.Models.JsonTypes.OmniaJsonBase
    {
        public Guid OwnerLayoutId { get; set; }
        public Guid Id { get; set; }
        public string Itemtype { get; set; }
        public List<LayoutItem> Items { get; set; }
        public Guid? ContainerId { get; set; }
        public Guid? PrevSiblingId { get; set; }
    }
}

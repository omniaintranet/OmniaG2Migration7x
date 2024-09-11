using Omnia.Fx.Models.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{

    public class PageLayoutData
    {
        public LayoutData Layout { get; set; }
        public Dictionary<Guid, BlockData> BlockData { get; set; }

        /// <summary>
        /// The page id of the page used as layout parent, or none
        /// </summary>
        public int? ParentLayoutPageId { get; set; }
    }
}

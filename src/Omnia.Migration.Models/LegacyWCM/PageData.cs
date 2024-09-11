using Newtonsoft.Json;
using Omnia.Fx.Models.Layouts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.LegacyWCM
{
    [JsonConverter(typeof(PageDataJsonConverter))]
    public class PageData: WebContentManagement.Models.Pages.PageData
    {
        // This is the legacy model from WCM 5.x, we are using this as the format for the input data (exported from G1)
        public PageLayoutData LayoutData { get; set; }

        // This is the current model used in Omnia WCM 6.x
        public Layout Layout { get; set; }
    }
}

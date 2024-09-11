using Omnia.Fx.Models.Layouts;
using Omnia.WebContentManagement.Models.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Factories
{
    //Layout factoty copied from WCM
    public class LayoutDataFactory
    {
        public static LayoutDefinition New()
        {
            Guid id = Guid.NewGuid();
            LayoutDefinition result = New(id);
            result.Id = id;
            return result;
        }

        public static LayoutDefinition New(Guid ownerLayoutId)
        {
            LayoutDefinition result = new LayoutDefinition()
            {
                //Cleaned = true,
                OwnerLayoutId = ownerLayoutId,
                Id = Guid.NewGuid(),
                //Itemtype = WcmCoreConstants.ContenStructure.LayoutItemType.layout,
                Itemtype = "layout",
                Items = new List<LayoutItem>(),
            };
            return result;
        }
    }
}

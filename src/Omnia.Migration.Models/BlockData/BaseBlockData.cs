using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.BlockData
{
    public abstract class BaseBlockData : Omnia.Migration.Models.LegacyWCM.BlockData
    {
        public abstract string GetElementName();
    }
}

using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.BlockData
{
    public class G1BlockSetting: OmniaJsonBase
    {
        public Guid ControlId { get; set; }

        public Guid InstanceId { get; set; }

        public string Scope { get; set; }        

        public string ZoneId { get; set; }

        public bool IsStatic { get; set; }
    }    
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class OmniaG1Settings
    {
        public string FoundationUrl { get; set; }
        public string IntranetUrl { get; set; }
        public string ODMUrl { get; set; }
        public string ExtensionId { get; set; }
        public string ApiSecret { get; set; }
        public string TokenKey { get; set; }
        public string TenantId { get; set; }
    }
}

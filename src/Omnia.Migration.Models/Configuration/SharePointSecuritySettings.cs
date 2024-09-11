using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class SharePointSecuritySettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthCookie { get; set; }
    }
}

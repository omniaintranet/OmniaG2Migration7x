using Microsoft.ProjectServer.Client;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Apps;
using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.MigrationItem
{
    public class SiteMigrationItem : OmniaJsonBase
    {
        public string SiteUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public uint LCID { get; set; }

        public int TimeZoneId { get; set; }

        public Guid GroupId { get; set; }

        public string GroupAlias { get; set; }

        public string GroupType { get; set; }

        public Dictionary<Guid, JObject> FeatureProperties { get; set; }

        public Dictionary<string, object> EnterpriseProperties { get; set; }

        public AppInstanceIdentities PermissionIdentities { get; set; }

        public string G1SiteTemplateId { get; set; }

        public SiteMigrationItem()
        {
            FeatureProperties = new Dictionary<Guid, JObject>();
            //hieu rem
            //PermissionIdentities = new AppInstanceIdentities() { Admin = new List<string>() };
            PermissionIdentities = new AppInstanceIdentities() { Admin = new List<Fx.Models.Identities.Identity>() };
        }

        public string SPTemplate { get; set; }
        public bool? IsPublic { get; set; }
    }

    public class SiteMigrationItem1 : OmniaJsonBase
    {
        public string SiteUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public uint LCID { get; set; }

        public int TimeZoneId { get; set; }

        public Guid GroupId { get; set; }

        public string GroupAlias { get; set; }

        public string GroupType { get; set; }

        public Dictionary<Guid, JObject> FeatureProperties { get; set; }

        public Dictionary<string, object> EnterpriseProperties { get; set; }
        public sPermissionIdentities PermissionIdentities { get; set; }
        public string G1SiteTemplateId { get; set; }
        public string SPTemplate { get; set; }
        public bool? IsPublic { get; set; }
    }
    public class sPermissionIdentities
    {
        public List<string> Admin { get; set; }

    }
}

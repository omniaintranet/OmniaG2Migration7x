using Newtonsoft.Json;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.BlockData;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Mappings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Core.Reports
{
    public class ExportSitesReport : BaseMigrationReport
    {
        #region Constructor

        static ExportSitesReport()
        { }

        private ExportSitesReport()
        {

        }

        public static ExportSitesReport Instance { get; } = new ExportSitesReport();

        #endregion

        public int NumberOfSites => ModernSites.Count + ClassicSites.Count + FailedSites.Count;

        public int NumberOfSucceedSites => ModernSites.Count;

        public int NumberOfFailedSites => FailedSites.Count;

        public int NumberOfPublicSites => PublicSites.Count;

        public int NumberOfNonPublicSites => NonPublicSites.Count;

        public int NumberOfNullPublicSites => NullPublicSites.Count;

        public int NumberOfNoG1SiteTemplates => NoG1SiteTemplates.Count;

        public List<string> NumberOfG1SiteTemplates => new List<string>(G1SiteTemplates.Select(x => x.Key + ": " + x.Value.Count).ToList()) { $"total: {ModernSites.Where(x => x.G1SiteTemplateId != null).Count()}" };

        [JsonIgnore]
        public List<SiteMigrationItem> ModernSites { get; set; }

        public List<string> SucceedSites { get { return ModernSites.Select(x => x.SiteUrl).ToList(); } }

        public List<string> PublicSites { get { return ModernSites.Where(x => x.IsPublic.HasValue && x.IsPublic.Value == true).Select(x => x.SiteUrl).ToList(); } }

        public List<string> NonPublicSites { get { return ModernSites.Where(x => x.IsPublic.HasValue && x.IsPublic.Value == false).Select(x => x.SiteUrl).ToList(); } }

        public List<string> NullPublicSites { get { return ModernSites.Where(x => !x.IsPublic.HasValue).Select(x => x.SiteUrl).ToList(); } }

        public List<string> ClassicSites { get; set; }

        public List<SitesReportFailedItem> FailedSites { get; set; }

        public List<Foundation.Models.Sites.SiteTemplate> SiteTemplates { get; set; }

        public Dictionary<Guid, List<string>> SiteTemplateProperties { get; set; }

        public Dictionary<string, List<string>> SPSiteTemplates { get { return ModernSites.Where(x => x.SPTemplate != null).GroupBy(x => x.SPTemplate).ToDictionary(x => x.Key, x => x.ToList().Select(x => x.SiteUrl).ToList()); } }

        public Dictionary<string, List<string>> G1SiteTemplates { get { return ModernSites.Where(x => x.G1SiteTemplateId != null).GroupBy(x => x.G1SiteTemplateId).ToDictionary(x => x.Key, x => x.ToList().Select(x => x.SiteUrl).ToList()); } }

        public List<string> NoG1SiteTemplates { get { return ModernSites.Where(x => x.G1SiteTemplateId == null).Select(x => x.SiteUrl).ToList(); } }

        public override string ReportName => "ExportSites";

        public override void Init(MigrationSettings settings)
        {
            base.Init(settings);

            ModernSites = new List<SiteMigrationItem>();
            ClassicSites = new List<string>();
            FailedSites = new List<SitesReportFailedItem>();
            SiteTemplates = new List<Foundation.Models.Sites.SiteTemplate>();
            SiteTemplateProperties = new Dictionary<Guid, List<string>>();
        }

        public void AddSiteTemplate(Foundation.Models.Sites.SiteTemplate template)
        {
            SiteTemplates.Add(template);
            SiteTemplateProperties.Add(template.Id, template.CustomFields.Select(x => x.InternalName).ToList());
        }

        public void AddFailedSite(string siteUrl, Exception exception)
        {
            FailedSites.Add(new SitesReportFailedItem(siteUrl, exception));
        }

        public override void ExportTo(string path)
        {
            base.ExportTo(path);

            string filePath = Path.Combine(path, GetDataFileName());
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this.ModernSites, Formatting.Indented));
        }

        private string GetDataFileName()
        {
            return GetReportFileName().Replace("Report.", "Data.");
        }
    }
}

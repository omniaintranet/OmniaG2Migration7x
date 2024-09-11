using Omnia.Fx.Models.AppSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    [AppSettingSectionName("MigrationSettings")]
    public class MigrationSettings: IAppSettingsSection
    {
        public string Customer { get; set; }

        public string InputPath { get; set; }

        public string OutputPath { get; set; }

        public bool UseCustomImageClient { get; set; }

        public WCMContextSettings WCMContextSettings { get; set; }

        public WorkplaceContextSettings WorkplaceContextSettings { get; set; }

        public ImportPagesSettings ImportPagesSettings { get; set; }

        public ImportLinksSettings ImportLinksSettings { get; set; }

        public ImportMyLinksSettings ImportMyLinksSettings { get; set; }

        public ExportSitesSettings ExportSitesSettings { get; set; }

        public ImportSitesSettings ImportSitesSettings { get; set; }

        public OmniaSecuritySettings OmniaSecuritySettings { get; set; }

        public OmniaG1Settings OmniaG1Settings{ get; set; }

        public SharePointSecuritySettings SharePointSecuritySettings { get; set; }

        public CustomHttpImageClientSettings CustomImageClient { get; set; }

        public MigrateCustomLink MigrateCustomLink { get; set; }
        public string OmniaTokenKey { get; set; }
        public AppInstanceSettings AppInstanceSettings { get; set; }

        public MigrationSettings()
        {
            WCMContextSettings = new WCMContextSettings();
            WorkplaceContextSettings = new WorkplaceContextSettings();                
            ImportPagesSettings = new ImportPagesSettings();
            ImportLinksSettings = new ImportLinksSettings();
            ImportMyLinksSettings = new ImportMyLinksSettings();
            ImportSitesSettings = new ImportSitesSettings();
            ExportSitesSettings = new ExportSitesSettings();
            OmniaSecuritySettings = new OmniaSecuritySettings();
            OmniaG1Settings = new OmniaG1Settings();
            SharePointSecuritySettings = new SharePointSecuritySettings();
            MigrateCustomLink = new MigrateCustomLink();
            AppInstanceSettings = new AppInstanceSettings();
        }
    }
}

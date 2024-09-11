using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class ImportSitesSettings: ParallelizableMigrationActionSettings
    {
        public string InputFilterFile { get; set; }
        public bool UpdateSite { get; set; }
        public string AppAdminnistrator { get; set; }
        public string BusinessProfileId { get; set; }
        public List<string> G1TemplatesToMigrate { get; set; }

        public ImportSitesSettings()
        {
            G1TemplatesToMigrate = new List<string>();                
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class ImportPagesSettings: ParallelizableMigrationActionSettings
    {
        public bool ImportLikesAndComments { get; set; }

        public bool ImportTranslationPages { get; set; }

        public bool MigrateImages { get; set; }

        //public bool UpdateExistingPages { get; set; }

        public bool ImportPageContent { get; set; }

        public bool ImportBlockSettings { get; set; }

        public string InputFilterFile { get; set; }

        public ImportPagesSettings()
        {

        }
    }
}

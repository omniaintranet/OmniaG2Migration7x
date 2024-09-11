using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Models.Configuration
{
    public class AppInstanceSettings: ParallelizableMigrationActionSettings
    {
        //public string InputFile { get; set; }
        public List<string> FeatureId { get; set; }
        public List<string> AppAdminnistrator { get; set; }
        public AppInstanceSettings()
        {
            FeatureId = new List<string>();
            AppAdminnistrator = new List<string>();
        }
    }
}

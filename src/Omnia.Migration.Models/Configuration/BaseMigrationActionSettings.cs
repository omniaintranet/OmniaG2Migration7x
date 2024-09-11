using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Configuration
{
    public class BaseMigrationActionSettings
    {
        public string InputFile { get; set; }
        public string? ExportDate { get; set; }
    }

    public class ParallelizableMigrationActionSettings: BaseMigrationActionSettings
    {
        public int NumberOfParallelThreads { get; set; }
    }
}

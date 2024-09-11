using Newtonsoft.Json;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Omnia.Migration.Core.Reports
{
    public abstract class BaseMigrationReport
    {
        public DateTime StartedAt { get; protected set; }

        public DateTime FinishedAt { get; protected set; }

        public double DurationInMinutes { get; protected set; }

        public string Customer { get; protected set; }

        public abstract string ReportName { get; }

        public virtual void Init(MigrationSettings settings)
        {
            Customer = settings.Customer;

            StartedAt = DateTime.Now;
            DurationInMinutes = 0;
        }

        protected virtual string GetReportFileName()
        {
            return $"Report.{ReportName}.{Customer}.{DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss")}.json";
        }

        public virtual void ExportTo(string path)
        {
            FinishedAt = DateTime.Now;
            DurationInMinutes = (FinishedAt - StartedAt).TotalMinutes;
            Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, GetReportFileName());
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}

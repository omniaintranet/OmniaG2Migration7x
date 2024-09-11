using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;

namespace Omnia.Migration.Core.Reports
{
    public class ImportLinksReport : BaseMigrationReport
    {
        #region Constructor

        static ImportLinksReport()
        { }

        private ImportLinksReport()
        {

        }

        public static ImportLinksReport Instance { get; } = new ImportLinksReport();

        #endregion

        #region Properties

        public override string ReportName => "ImportLinks";

        public List<string> SucceedLinks { get; set; }

        public List<LinksReportFailedItem> FailedLinks { get; set; }

        public List<string> LinksAlreadyAttachedToG2 { get; set; }

        public List<string> LinksWithoutURL { get; set; }

        #endregion

        #region Methods

        public override void Init(MigrationSettings settings)
        {
            base.Init(settings);
            SucceedLinks = new List<string>();
            FailedLinks = new List<LinksReportFailedItem>();
            LinksAlreadyAttachedToG2 = new List<string>();
            LinksWithoutURL = new List<string>();            
        }

        public void AddSucceedLink(string Url)
        {
            SucceedLinks.Add(Url);
        }

        public void AddFailedLink(string Url, Exception exception)
        {
            FailedLinks.Add(new LinksReportFailedItem(Url, exception));
        }

        public void AddLinkAlreadyAttached(string Url)
        {
            LinksAlreadyAttachedToG2.Add(Url);
        }       

        public void AddLinkWithoutURL(string LinkId)
        {
            LinksWithoutURL.Add(LinkId);
        }

        #endregion
    }

    public class LinksReportFailedItem
    {
        public string linkUrl { get; set; }

        public string Exception { get; set; }

        public LinksReportFailedItem(string Url, Exception exception = null)
        {
            linkUrl = Url;
            Exception = exception != null ? exception.Message + exception.StackTrace : null;
        }
    }
}

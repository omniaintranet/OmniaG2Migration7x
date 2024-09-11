using Microsoft.Graph;
using Newtonsoft.Json;
using Omnia.Fx.Models.Identities;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.Text;
using Identity = Omnia.Fx.Models.Identities.Identity;

namespace Omnia.Migration.Core.Reports
{
	public class ImportSitesReport : BaseMigrationReport
	{
		#region Constructor

		static ImportSitesReport()
		{ }

		private ImportSitesReport()
		{

		}

		public static ImportSitesReport Instance { get; } = new ImportSitesReport();

		#endregion

		#region Properties

		public override string ReportName => "ImportSites";

		public List<string> SucceedSites { get; set; }

		public List<SitesReportFailedItem> FailedSites { get; set; }

		public List<string> SitesAlreadyAttachedToG2 { get; set; }
		public List<string> SitesNotAttachedToG2 { get; set; }
		public List<string> UpdatePermissionsSucceedSites { get; set; }
		public List<string> UpdatePermissionsFailedSites { get; set; }
		public List<string> SitesWithoutMappedTemplate { get; set; }

		public List<SitesReportFailedUser> FailedUsers { get; set; }

		#endregion

		#region Methods

		public override void Init(MigrationSettings settings)
		{
			base.Init(settings);
			SucceedSites = new List<string>();
			FailedSites = new List<SitesReportFailedItem>();
			SitesAlreadyAttachedToG2 = new List<string>();
			SitesNotAttachedToG2 = new List<string>();
			UpdatePermissionsSucceedSites = new List<string>();
			UpdatePermissionsFailedSites = new List<string>();
			SitesWithoutMappedTemplate = new List<string>();
			FailedUsers = new List<SitesReportFailedUser>();
		}

		public void AddSucceedSite(SiteMigrationItem siteMigrationItem)
		{
			SucceedSites.Add(siteMigrationItem.SiteUrl);
		}

		public void AddFailedSite(SiteMigrationItem siteMigrationItem, Exception exception)
		{
			FailedSites.Add(new SitesReportFailedItem(siteMigrationItem.SiteUrl, exception));
		}

		public void AddSiteAlreadyAttached(SiteMigrationItem siteMigrationItem)
		{
			SitesAlreadyAttachedToG2.Add(siteMigrationItem.SiteUrl);
		}
		public void AddSiteNOTAttachedYet(string siteURL)
		{
			SitesNotAttachedToG2.Add(siteURL);
		}
		public void AddUpdatePermissionsSucceedSites(string siteURL)
        {
			UpdatePermissionsSucceedSites.Add(siteURL);
        }
		public void AddUpdatePermissionsFailedSites(string siteURL)
		{
			UpdatePermissionsFailedSites.Add(siteURL);
		}
		public void AddSiteWithoutTemplate(SiteMigrationItem siteMigrationItem)
		{
			SitesWithoutMappedTemplate.Add(siteMigrationItem.SiteUrl);
		}

		//Hieu rem
        //public void AddFailedUser(SiteMigrationItem siteMigrationItem, List<string> failedUsers)
        //{
        //    FailedUsers.Add(new SitesReportFailedUser(siteMigrationItem.SiteUrl, failedUsers));
        //}
        public void AddFailedUser(SiteMigrationItem siteMigrationItem, List< Identity> failedUsers)
		{
			FailedUsers.Add(new SitesReportFailedUser(siteMigrationItem.SiteUrl, failedUsers));
		}

		#endregion
	}

	public class SitesReportFailedItem
    {
        public string SiteUrl { get; set; }

        public string Exception { get; set; }

        public SitesReportFailedItem(string siteUrl, Exception exception = null)
        {
            SiteUrl = siteUrl;
            Exception = exception != null ? exception.Message + exception.StackTrace : null;
        }
    }
    public class SitesReportFailedUser
    {
        public string SiteUrl { get; set; }

        public string Exception { get; set; }

        public SitesReportFailedUser(string siteUrl, List<Identity> emails)
        {
            SiteUrl = siteUrl;
            Exception = "Failed to resolve the following users: " + string.Join(",", emails);
        }
    }

}

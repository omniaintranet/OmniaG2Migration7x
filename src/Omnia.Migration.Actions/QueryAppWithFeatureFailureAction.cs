using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Actions
{
	public class QueryAppWithFeatureFailureAction : BaseMigrationAction
	{
		private AppApiHttpClient AppApiHttpClient { get; }
		private IProgressManager ProgressManager { get; set; }
		private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
		private List<Omnia.Fx.Models.Apps.AppInstance> ErrorAppInstances { get; set; }

		public QueryAppWithFeatureFailureAction(
			IOptionsSnapshot<MigrationSettings> migrationSettings,
			AppApiHttpClient appApiHttpClient)
		{
			MigrationSettings = migrationSettings;
			AppApiHttpClient = appApiHttpClient;
		}
		public override async Task StartAsync(IProgressManager progressManager)
		{
			ProgressManager = progressManager;
			List<SiteMigrationItem> input = ReadInput();

			ProgressManager.Start(1);
			ImportSitesReport.Instance.Init(MigrationSettings.Value);

			try
			{
				await LoadErrorAppInstancesAsync();

				if (ErrorAppInstances.Count > 0)
				{
					var result = ErrorAppInstances.Where(error => input.Any(i => i.SiteUrl == error.OutputInfo.AbsoluteAppUrl));

					foreach (var rs in result)
					{
						ImportSitesReport.Instance.AddFailedSite(new SiteMigrationItem() { SiteUrl = rs.OutputInfo.AbsoluteAppUrl }, new Exception(rs.Error));
					}
				}
				
				ProgressManager.ReportProgress(1);
			}
			catch (Exception ex)
			{
				ImportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
				throw;
			}
			finally
			{
				ImportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
			}
		}

		private List<SiteMigrationItem> ReadInput()
		{
			var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportSitesSettings.InputFile);
			var input = JsonConvert.DeserializeObject<List<SiteMigrationItem>>(System.IO.File.ReadAllText(inputPath));

			if (MigrationSettings.Value.ImportSitesSettings.G1TemplatesToMigrate.Count > 0)
			{
				input = input.Where(site => MigrationSettings.Value.ImportSitesSettings.G1TemplatesToMigrate.Any(x => x.ToLower() == site.G1SiteTemplateId?.ToLower())).ToList();
			}

			return input;
		}
		private async Task LoadErrorAppInstancesAsync()
		{
			ErrorAppInstances = new List<Omnia.Fx.Models.Apps.AppInstance>();
			var businessProfiles = MigrationSettings.Value.WCMContextSettings.SiteTemplateMappings.Select(x => x.BusinessProfileId).Distinct();
			foreach (var profile in businessProfiles)
			{
				var appInstancesResult = await AppApiHttpClient.GetAppInstancesAsync(Core.Constants.AppDefinitionIDs.TeamCollaborationDefinitionID, profile, false, true);
				appInstancesResult.EnsureSuccessCode();

				
				ErrorAppInstances.AddRange(appInstancesResult.Data.AppInstances);
			}
		}
	}
}

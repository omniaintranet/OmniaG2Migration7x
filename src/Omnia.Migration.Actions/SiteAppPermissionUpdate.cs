using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Apps;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Services;
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
	public class SiteAppPermissionUpdate : BaseMigrationAction
	{
		private AppApiHttpClient AppApiHttpClient { get; }
		private IProgressManager ProgressManager { get; set; }
		private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
		private List<Omnia.Fx.Models.Apps.AppInstance> ExistingAppInstances { get; set; }

		public SiteAppPermissionUpdate(
			IOptionsSnapshot<MigrationSettings> migrationSettings,
			AppApiHttpClient appApiHttpClient)
		{
			MigrationSettings = migrationSettings;
			AppApiHttpClient = appApiHttpClient;
		}
		public override async Task StartAsync(IProgressManager progressManager)
		{
			Console.WriteLine("   Select action:....");
			Console.WriteLine("		1. Add app admin...");
			Console.WriteLine("     2. Remove app admin...");
			string action = Console.ReadLine();

			ProgressManager = progressManager;
			string[] input = ReadInput();

			ProgressManager.Start(input.Count());
			ImportSitesReport.Instance.Init(MigrationSettings.Value);

			try
			{
				await LoadExistingAppInstancesAsync();

				foreach (var site in input)
				{
					Fx.Models.Apps.AppInstance appInstance = new Fx.Models.Apps.AppInstance();
					var appList = ExistingAppInstances.Where(x => x.HasSPUrl(site));
					if (appList.Count() > 1)
					{
						appInstance = ExistingAppInstances.FirstOrDefault(x => x.HasSPUrl(site) && x.DefaultResourceUrl != null);
					}
					else
					{
						appInstance = ExistingAppInstances.SingleOrDefault(x => x.HasSPUrl(site));
					}

					if (appInstance != null)
					{
						await UpdateSitesPermissionAsync(appInstance, action);

					}
					else
					{
						////TODO: Add to report							
						ImportSitesReport.Instance.AddSiteNOTAttachedYet(site);
					}
				}

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

		private string[] ReadInput()
		{
			var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.AppInstanceSettings.InputFile);
			string[] input = (System.IO.File.ReadAllLines(inputPath));

			return input;
		}
		private async Task LoadExistingAppInstancesAsync()
		{
			ExistingAppInstances = new List<Omnia.Fx.Models.Apps.AppInstance>();
			var businessProfiles = MigrationSettings.Value.WCMContextSettings.SiteTemplateMappings.Select(x => x.BusinessProfileId).Distinct();
			foreach (var profile in businessProfiles)
			{
				var appInstancesResult = await AppApiHttpClient.GetAppInstancesAsync(Core.Constants.AppDefinitionIDs.TeamCollaborationDefinitionID, profile, false, false);
				appInstancesResult.EnsureSuccessCode();
				ExistingAppInstances.AddRange(appInstancesResult.Data.AppInstances);
			}
		}
		private async Task UpdateSitesPermissionAsync(Fx.Models.Apps.AppInstance site, string action)
		{
			try
			{
				foreach (var appAdmin in MigrationSettings.Value.AppInstanceSettings.AppAdminnistrator)
				{
					if (action == "1")
					{
						site.PermissionIdentities.Admin.Add(appAdmin);
					}
					else
					{
						//Diem - 08112022: Remove user in Admin app  
						var listAmdin = site.PermissionIdentities.Admin;
						int countofAdmin = listAmdin.Count();
						for (int i = 0; i < countofAdmin; i++)
						{
                            //Hieu rem
                            //if (listAmdin[i].ToLower() == appAdmin.ToLower())
                            if (listAmdin[i].Id.ToString().ToLower() == appAdmin.ToLower())
                            {
								site.PermissionIdentities.Admin.Remove(listAmdin[i]);
								break;
							}
							if (listAmdin.Count() - 1 <= i)
							{
								break;
							}
						}
					}
				}

				JObject payLoad = new JObject(
					new JProperty("permissionSettings",
						new JObject(
							new JProperty("roleId", "f17d076c-d46b-43fd-94e2-e664dd43ed92"),
							new JProperty("identities", site.PermissionIdentities.Admin),
							new JProperty("groups", new JArray()),
							new JProperty("userDefinedRules", new JArray())),
						new JObject(
							new JProperty("roleId", "e5ba2879-76a1-4d0a-ab71-6c7ca6cd4791"),
							new JProperty("identities", new JArray()),
							new JProperty("groups", new JArray()),
							new JProperty("userDefinedRules", new JArray()))),
					new JProperty("permissionContextParam",
						new JObject(
							new JProperty("profileid", site.BusinessProfileId),
							new JProperty("appinstanceid", site.Id)))

					);

				var updateAppInstanceResult = await AppApiHttpClient.UpdateAppInstancePermissionAsync(
				appInstanceId: site.Id,
				profileId: site.BusinessProfileId,
				payLoad: payLoad);
				updateAppInstanceResult.EnsureSuccessCode();
				
				ImportSitesReport.Instance.AddUpdatePermissionsSucceedSites(site.DefaultResourceUrl);
				ProgressManager.ReportProgress(1);
			}
			catch (Exception e)
			{
				//Logger.LogError(e.Message);
				ImportSitesReport.Instance.AddUpdatePermissionsFailedSites(site.DefaultResourceUrl);
			}

		}
	}
}

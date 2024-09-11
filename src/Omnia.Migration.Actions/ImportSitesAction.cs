using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Extensions;
using Microsoft.SharePoint.Client;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models.Mappings;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Apps;
using System.Xml.Linq;
using Omnia.Fx.Models.Language;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;

namespace Omnia.Migration.Actions
{
	public class TempAppInstanceInfo : Fx.Models.Apps.AppInstanceInputInfo
	{
		public Dictionary<string, object> EnterpriseProperties { get; set; }

		public TempAppInstanceInfo()
		{
			EnterpriseProperties = new Dictionary<string, object>();
		}
	}

	public class ImportSitesAction : ParallelizableMigrationAction
	{
		private AppApiHttpClient AppApiHttpClient { get; }
		private G1FeatureApiHttpClient FeatureApiHttpClient { get; }
		private SPTokenService SPTokenService { get; }
		private SitesService SitesService { get; }
		private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
		private ILogger<ImportSitesAction> Logger { get; }

		private IProgressManager ProgressManager { get; set; }
		private IdentityApiHttpClient IdentityApiHttpClient { get; }

        public ItemQueryResult<IResolvedIdentity> Identities { get; set; }
        private List<Omnia.Fx.Models.Apps.AppInstance> ExistingAppInstances { get; set; }
		private List<string> selectedPersonProperties { get; set; }
		private string actionFlag { get; set; }
		public ImportSitesAction(
			AppApiHttpClient appApiHttpClient,
			G1FeatureApiHttpClient featureApiHttpClient,
            IdentityApiHttpClient identityApiHttpClient,
            SPTokenService spTokenService,
			SitesService sitesService,
			IOptionsSnapshot<MigrationSettings> migrationSettings,
			ILogger<ImportSitesAction> logger)
		{
			AppApiHttpClient = appApiHttpClient;
			FeatureApiHttpClient = featureApiHttpClient;
			SPTokenService = spTokenService;
			SitesService = sitesService;
			MigrationSettings = migrationSettings;
			Logger = logger;
			IdentityApiHttpClient = identityApiHttpClient;

        }

		public override async Task StartAsync(IProgressManager progressManager)
		{
			Console.WriteLine("Team site migration........");
			//Console.WriteLine("   Select action:....");
			//Console.WriteLine("		1. Import Site...");
			//Console.WriteLine("     2. Get Test sites...");
			//string action = Console.ReadLine();
			Console.WriteLine("     1. Full Import...");
			Console.WriteLine("     2. Import site by filter file...");
			string filterOption = Console.ReadLine();

			Console.WriteLine("   Delete app instance with URL NULL? Y/N...    ");
			actionFlag = Console.ReadLine();
			//Hieu added
            var users = await IdentityApiHttpClient.GetUserIdentitybyEmail("");
            if (users.Success)
            {
                this.Identities = users.Data;
            }
            else
            {
               // Logger.Log("Cannot get user identities, stop task!: " + users.ErrorMessage.ToString());
                return;
            }
			//<<

            ProgressManager = progressManager;
			List<SiteMigrationItem> input = ReadInput();

			//Run only one site
			//input = input.Where(x => x.SiteUrl.ToString() == "https://onenordic.sharepoint.com/sites/19772-Hgans-Energi-fiberutbyggnad-Hgans-Nedre").ToList();

			if (filterOption == "2")
			{
				input = FilterInput(input);
			}

			selectedPersonProperties = new List<string>();
			selectedPersonProperties = SiteHelper.SelectPersonProperties(input);

			ProgressManager.Start(input.Count);
			ImportSitesReport.Instance.Init(MigrationSettings.Value);

			try
			{
				await LoadExistingAppInstancesAsync();

				RunInParallel(input, MigrationSettings.Value.ImportSitesSettings.NumberOfParallelThreads, MigrateSitesAsync);
			}
			catch (Exception)
			{
				ImportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
				throw;
			}
			finally
			{
				ImportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
			}
		}

		private async Task LoadExistingAppInstancesAsync()
		{
			ExistingAppInstances = new List<Omnia.Fx.Models.Apps.AppInstance>();
			var businessProfiles = MigrationSettings.Value.WCMContextSettings.SiteTemplateMappings.Select(x => x.BusinessProfileId).Distinct();
			foreach (var profile in businessProfiles)
			{
				var appInstancesResult = await AppApiHttpClient.GetAppInstancesAsync(Core.Constants.AppDefinitionIDs.TeamCollaborationDefinitionID, profile);
				appInstancesResult.EnsureSuccessCode();

				//ExistingAppInstances.AddRange(appInstancesResult.Data.AppInstances.Where(x => x.Status == Fx.Models.Apps.AppInstanceStatus.Ready).ToList());
				ExistingAppInstances.AddRange(appInstancesResult.Data.AppInstances.ToList());
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
		private List<SiteMigrationItem> FilterInput(List<SiteMigrationItem> input)
		{
			List<SiteMigrationItem> filterInput = new List<SiteMigrationItem>();
			var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportSitesSettings.InputFilterFile);
			string[] lines = System.IO.File.ReadAllLines(@inputPath);
			foreach (string line in lines)
			{
				var filter = input.Where(x => x.SiteUrl.ToString() == line.ToString()).ToList();
				filterInput.AddRange(filter);
			}
			return filterInput;
		}
		private async Task MigrateSitesAsync(List<SiteMigrationItem> sites)
		{
			var siteTemplateMappings = MigrationSettings.Value.WCMContextSettings.SiteTemplateMappings.ToDictionary(x => x.G1TemplateId.ToString().ToLower());
			foreach (var site in sites)
			{
				try
				{
					SiteTemplateMapping siteTemplateMapping = SitesService.GetSiteTemplateMapping(siteTemplateMappings, site);
					var clientContext = await SPTokenService.CreateAppOnlyClientContextAsync(site.SiteUrl);

					if(actionFlag !="Y")
                    {
						//Hieu updated
						await SitesService.EnsureFailedUser(clientContext, site, selectedPersonProperties, Identities);
					}					

					var omniaCustomAction = await SitesService.GetOmniaG2CustomActionAsync(clientContext);
					
					var canImportSite = true;
					if (omniaCustomAction.Count == 1 && MigrationSettings.Value.ImportSitesSettings.UpdateSite == false)
					{
						ImportSitesReport.Instance.AddSiteAlreadyAttached(site);
						canImportSite = false;
					}
					if (siteTemplateMapping == null)
					{
						ImportSitesReport.Instance.AddSiteWithoutTemplate(site);
						canImportSite = false;
					}

					if (canImportSite)
					{
						//Diem - 07 - Jul - 2022: Remove invalid user in Admin app and Add migration account for feature enable    
						var listAmdin = site.PermissionIdentities.Admin;
						int countofAdmin = listAmdin.Count();
						for (int i = 0; i < countofAdmin; i++)
						{
							//Hieu rem: Should update here
							var existUser = ImportSitesReport.Instance.FailedUsers.Where(x => x.Exception.ToString().Contains(listAmdin[i]));

							if (existUser.Count() > 0)
							{
								site.PermissionIdentities.Admin.Remove(listAmdin[i]);
							}
							if (listAmdin.Count() - 1 <= i)
							{
								break;
							}
						}
						if (!string.IsNullOrEmpty(MigrationSettings.Value.ImportSitesSettings.AppAdminnistrator))
						{
							site.PermissionIdentities.Admin.Add(MigrationSettings.Value.ImportSitesSettings.AppAdminnistrator);
						}

						dynamic createSiteProperties = SitesService.GenerateCreateSiteProperties(site);
						Dictionary<string, object> g2EnterpriseProps = MapEnterpriseProperties(site, siteTemplateMapping);

                        //Hieu add
                        LanguageTag defaultLang = new LanguageTag();
						//<<
                        var appInstanceInfo = new TempAppInstanceInfo
                        {
							//Hieu rem
                            //Title = site.Title,
                            //Description =  site.Description,
                            //ShowInPublicListings = site.IsPublic.HasValue ? site.IsPublic.Value : false, 
							ShowInPublicListings = site.IsPublic.HasValue ? ShowInPublicListingsMode.PublicToAppViewer : ShowInPublicListingsMode.None,
                            Properties = createSiteProperties,
							FeatureProperties = site.FeatureProperties,
							EnterpriseProperties = g2EnterpriseProps,
							PermissionIdentities = site.PermissionIdentities
						};
                        //>>Hieu added
                        appInstanceInfo.Title.Add(defaultLang, site.Title);
                        appInstanceInfo.Description.Add(defaultLang, site.Title);
                        //<<Hieu added


                        if (MigrationSettings.Value.ImportSitesSettings.UpdateSite)
						{
							var json = JsonConvert.SerializeObject(appInstanceInfo);
							var appInstanceInfo2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
							Fx.Models.Apps.AppInstance appInstance = new Fx.Models.Apps.AppInstance();
							var appList = ExistingAppInstances.Where(x => x.HasSPUrl(site.SiteUrl));
							if(appList.Count() > 1)
                            {
								appInstance = ExistingAppInstances.FirstOrDefault(x => x.HasSPUrl(site.SiteUrl) && x.DefaultResourceUrl == null);
							}
                            else
                            {
								appInstance = ExistingAppInstances.SingleOrDefault(x => x.HasSPUrl(site.SiteUrl));
							}		
							
							if (appInstance != null && actionFlag !="Y" && actionFlag.IsNotNull())
							{
								var updateAppInstanceResult = await AppApiHttpClient.UpdateAppInstanceEnterprisePropertiesAsync(
								appInstanceId: appInstance.Id,
								profileId: siteTemplateMapping.BusinessProfileId,
								spUrl: site.SiteUrl,
								//appInstanceProperties: appInstanceInfo.EnterpriseProperties);
								appInstanceProperties: appInstanceInfo2);

								updateAppInstanceResult.EnsureSuccessCode();
								ImportSitesReport.Instance.AddSucceedSite(site);
							}
							else if (appInstance != null && actionFlag == "Y")
							{
								//TODO: Add to report
								var deleteAppInstanceResult = await AppApiHttpClient.DeleteAppInstanceAsync(appInstanceId: appInstance.Id,
								profileId: siteTemplateMapping.BusinessProfileId);

								deleteAppInstanceResult.EnsureSuccessCode();
								ImportSitesReport.Instance.AddSucceedSite(site);
							}
                            else
                            {
								//TODO: Add to report
							}
						}
						else
						{
							var createAppInstanceResult = await AppApiHttpClient.CreateAppInstanceAsync(
								   profileId: siteTemplateMapping.BusinessProfileId,
								   appTemplateId: siteTemplateMapping.G2TemplateId,
								   siteUrl: site.SiteUrl,
								   inputInfo: appInstanceInfo);

							createAppInstanceResult.EnsureSuccessCode();

							await SitesService.RemoveG1SPFxHeaderAsync(clientContext);
							ImportSitesReport.Instance.AddSucceedSite(site);
						}						
					}

					ProgressManager.ReportProgress(1);
				}
				catch (Exception e)
				{
					Logger.LogError(e.Message);
					ImportSitesReport.Instance.AddFailedSite(site, e);
				}
			}
		}

		private Dictionary<string, object> MapEnterpriseProperties(SiteMigrationItem site, SiteTemplateMapping siteTemplateMapping)
		{
			var g2EnterpriseProps = new Dictionary<string, object>();
			foreach (var g1Prop in site.EnterpriseProperties)
			{
				if (siteTemplateMapping.Properties.ContainsKey(g1Prop.Key) && g1Prop.Value != null)
				{
					var oldValue = JToken.FromObject(g1Prop.Value);
					var propMapping = siteTemplateMapping.Properties[g1Prop.Key];

					//Diem - 30Jun2022: if user field value type - Recheck Quan
					if(propMapping.PropertyType == Models.EnterpriseProperties.EnterprisePropertyType.User)
                    {
                        try
                        {
							oldValue = JToken.FromObject(oldValue.Values<string>("Email"));
							if (oldValue.IsNull())
							{
								oldValue = JToken.FromObject(oldValue.Values<string>("LookupValue"));
							}
						}
						catch
                        {
							//nothing to do
							oldValue = JToken.FromObject(g1Prop.Value);
						}						
					}
					g2EnterpriseProps.Add(propMapping.PropertyName, EnterprisePropertyMapper.MapPropertyValue(oldValue, propMapping.PropertyType, MigrationSettings.Value.WCMContextSettings, Identities));
				}
			}

			return g2EnterpriseProps;
		}

		private void GetTestSiteOnly(List<SiteMigrationItem> sites)
        {
			List<string> listSite = new List<string>();
			foreach (var site in sites)
            {
				if(site.SiteUrl.Contains("test"))
                {
					listSite.Add(site.SiteUrl);
                }
            }
            System.IO.File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueyTestSite.json"), JsonConvert.SerializeObject(listSite, Formatting.Indented));
		}
	}
}

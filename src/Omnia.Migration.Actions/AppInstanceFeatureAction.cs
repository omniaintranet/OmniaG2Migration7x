using FeaturesAction.Features;
using Microsoft.Extensions.Options;
using Omnia.Migration.Core;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Models.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.Migration.Actions
{
    public class AppInstanceFeatureAction : BaseMigrationAction
    {
        private AppApiHttpClient AppApiHttpClient { get; }
        private SitesService SitesService { get; }
        private G1FeatureApiHttpClient FeatureApiHttpClient { get; }
        private IProgressManager ProgressManager { get; set; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        public List<SitesReportFailedItem> FailedSites { get; set; }
        private string actionFlag { get; set; }
        public AppInstanceFeatureAction(
            G1FeatureApiHttpClient featureApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            SitesService sitesService,
            AppApiHttpClient appApiHttpClient)
        {
            FeatureApiHttpClient = featureApiHttpClient;
            MigrationSettings = migrationSettings;
            AppApiHttpClient = appApiHttpClient;
            SitesService = sitesService;
            FailedSites = new List<SitesReportFailedItem>();
        }
        public override async Task StartAsync(IProgressManager progressManager)
        {
            Console.WriteLine("Team site - feature action........");
            Console.WriteLine("  Are you sure to remove G1 features? Y/N....");
            //Console.WriteLine("		1. Import Site...");
            //Console.WriteLine("     2. Get Test sites...");
            //string action = Console.ReadLine();
            actionFlag = Console.ReadLine();

            ProgressManager = progressManager;

            List<string> sitesToMigrates = ReadInput();

            progressManager.Start(sitesToMigrates.Count);
            ImportSitesReport.Instance.Init(MigrationSettings.Value);
            if (actionFlag == "Y" || actionFlag == "y")
            {
                StartDeactivateG1Feature(sitesToMigrates);
            }

        }
        private List<string> ReadInput()
        {
            List<string> inputReturn = new List<string>();
            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.AppInstanceSettings.InputFile);
            string[] inputStr = System.IO.File.ReadAllLines(@inputPath);
            inputReturn.AddRange(inputStr);
            //foreach(string line in inputStr)
            //{
            //    inputReturn.Add(line);
            //}
            return inputReturn;
        }
        private async Task<List<string>> QueryListOfSites()
        {
            var sitesToMigrates = new List<string>();
            sitesToMigrates.AddRange(await SitesService.GetG1SPFxSitesAsync());
            sitesToMigrates.AddRange(await SitesService.GetG1MasterPageSitesAsync());
            sitesToMigrates = sitesToMigrates.Distinct().ToList();
            return sitesToMigrates;
        }
        private void StartDeactivateG1Feature(List<string> sites)
        {
            var featureIds = new List<Guid>();
            featureIds.Add(Constants.G1FeatureIds.SpfxInfrastructure);
            featureIds.Add(Constants.G1FeatureIds.CoreMasterPage);
            featureIds.Add(Constants.G1FeatureIds.OmniaDocumentManagementAuthoringInfrastructure);
            featureIds.Add(Constants.G1FeatureIds.OmniaDocumentManagementAuthoringSite);
            featureIds.Add(Constants.G1FeatureIds.OmniaIntranetTeamSiteAnnouncements);
            featureIds.Add(Constants.G1FeatureIds.OmniaIntranetTeamSiteTasks);
            featureIds.Add(Constants.G1FeatureIds.OmniaIntranetTeamSiteCalendar);
            featureIds.Add(Constants.G1FeatureIds.OmniaIntranetTeamSiteLinks);
            featureIds.Add(Constants.G1FeatureIds.OmniaDocumentManagementCreateDocumentWizard);

            if (MigrationSettings.Value.AppInstanceSettings.FeatureId.Count > 0)
            {
                featureIds.Where(id => MigrationSettings.Value.AppInstanceSettings.FeatureId.Any(x => x.ToLower() == id.ToString().ToLower())).ToList();
            }
            foreach (var site in sites)
            {
                foreach (var id in featureIds)
                {
                    try
                    {
                        //Console.WriteLine("Site " + idx.ToString() + " : " + site.ToString());
                        var siteResult = DeactivateG1Feature(site, id);
                        if (siteResult.isSuccess)
                        {
                            ImportSitesReport.Instance.SucceedSites.Add(site);
                        }
                        else
                        {
                            ImportSitesReport.Instance.FailedSites.Add(new SitesReportFailedItem(site, siteResult.errorMessage));
                        }                        
                    }
                    catch (Exception e)
                    {
                        //Logger.LogError(e.Message);
                        ImportSitesReport.Instance.FailedSites.Add(new SitesReportFailedItem(site, e));
                    }
                }
                ProgressManager.ReportProgress(1);
            }
            ImportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
        }
        private ActivateResult DeactivateG1Feature(string spUrl, Guid featureId)
        {
            var getFeaturesResult = FeatureApiHttpClient.DeactivateFeatureAsync(featureId, spUrl);
            return getFeaturesResult;
        }
    }
}


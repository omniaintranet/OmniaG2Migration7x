using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Mappings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.Migration.Actions
{
    public class ExportSitesAction : ParallelizableMigrationAction
    {
        private G1FeatureApiHttpClient FeatureApiHttpClient { get; }
        private G1SiteTemplatesHttpClient SiteTemplatesHttpClient { get; }
        private SPTokenService SPTokenService { get; }
        private SitesService SitesService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<ExportSitesAction> Logger { get; }
        private IdentityApiHttpClient IdentityApiHttpClient { get; }

        private IProgressManager ProgressManager { get; set; }

        public ExportSitesAction(
            G1FeatureApiHttpClient featureApiHttpClient,
            G1SiteTemplatesHttpClient siteTemplatesHttpClient,
            SPTokenService spTokenService,
            SitesService sitesService,
            IdentityApiHttpClient identityApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            ILogger<ExportSitesAction> logger)
        {
            FeatureApiHttpClient = featureApiHttpClient;
            SiteTemplatesHttpClient = siteTemplatesHttpClient;
            SPTokenService = spTokenService;
            SitesService = sitesService;
            MigrationSettings = migrationSettings;
            Logger = logger;
            IdentityApiHttpClient = identityApiHttpClient;
        }

        public override async Task StartAsync(IProgressManager progressManager)
        {
            ProgressManager = progressManager;

            List<string> sitesToMigrates = await ReadInputAsync();

            //Diem: 01 Aug 2022 - get unique URL from the orginal list
            sitesToMigrates = sitesToMigrates.Distinct().ToList();

            progressManager.Start(sitesToMigrates.Count);
            ExportSitesReport.Instance.Init(MigrationSettings.Value);

            try
            {
                var siteTemplates = await SiteTemplatesHttpClient.GetSiteTemplates();
                siteTemplates.Data.ForEach(template => ExportSitesReport.Instance.AddSiteTemplate(template));

                RunInParallel(sitesToMigrates, MigrationSettings.Value.ExportSitesSettings.NumberOfParallelThreads, ExtractSitesAsync);
            }
            catch (Exception)
            {
                ExportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
                throw;
            }
            finally
            {
                ExportSitesReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
            }
        }

        private async ValueTask<List<string>> ReadInputAsync()
        {
            if (!string.IsNullOrEmpty(MigrationSettings.Value.ExportSitesSettings.InputFile))
            {
                var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ExportSitesSettings.InputFile);
                var inputStr = System.IO.File.ReadAllText(inputPath);

                try
                {
                    return JsonConvert.DeserializeObject<List<string>>(inputStr);
                }
                catch (Exception)
                {
                    var sites = JsonConvert.DeserializeObject<List<SiteMigrationItem>>(inputStr);
                    return sites.Select(x => x.SiteUrl).ToList();
                }
            }
            else
            {
                return await QueryListOfSites();
            }
        }

        private async Task ExtractSitesAsync(List<string> sitesToMigrates)
        {
            var siteTemplateMappings = MigrationSettings.Value.WCMContextSettings.SiteTemplateMappings.ToDictionary(x => x.G1TemplateId.ToString().ToLower());
            foreach (var siteUrl in sitesToMigrates)
            {
                ProgressManager.ReportProgress(1);

                try
                {
                    var clientContext = await SPTokenService.CreateAppOnlyClientContextAsync(siteUrl);
                    var siteOmniaCustomAction = await SitesService.GetOmniaG1CustomerActionAsync(clientContext);
                    //Comment out if client want to attach classic site that have been converted to modern

                    //if (siteOmniaCustomAction.Count == 0)
                    //{
                    //    ExportSitesReport.Instance.ClassicSites.Add(siteUrl);
                    //    continue;
                    //}

                    var siteOwnerGroup = clientContext.Web.AssociatedOwnerGroup;
                    var owners = siteOwnerGroup.Users;
                    clientContext.Load(clientContext.Web);
                    clientContext.Load(clientContext.Web);
                    clientContext.Load(clientContext.Web.AllProperties);
                    clientContext.Load(clientContext.Web.RegionalSettings);
                    clientContext.Load(clientContext.Web.RegionalSettings.TimeZone);
                    clientContext.Load(owners);
                    await clientContext.ExecuteQueryAsync();

                    var groupId = clientContext.Web.AllProperties.FieldValues.ContainsKey("GroupId") ? new Guid(clientContext.Web.AllProperties["GroupId"].ToString()) : Guid.Empty;
                    var groupAlias = clientContext.Web.AllProperties.FieldValues.ContainsKey("GroupAlias") ? clientContext.Web.AllProperties["GroupAlias"].ToString() : string.Empty;
                    var groupType = clientContext.Web.AllProperties.FieldValues.ContainsKey("GroupType") ? clientContext.Web.AllProperties["GroupType"].ToString() : string.Empty;
                    DateTime createdDate = clientContext.Web.Created;

                    //Diem - 07-Jul-2022: Filter by ExportDate in appsetting
                    if (MigrationSettings.Value.ExportSitesSettings.ExportDate.IsNotNull())
                    {
                        var minDate = DateTime.ParseExact(MigrationSettings.Value.ExportSitesSettings.ExportDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                        if (createdDate < minDate)
                        {
                            continue;
                        }
                    }

                    var siteDirectoryInfo = await SitesService.GetSiteDirectoryInfoFileAsync(clientContext);

                    string siteTemplateId = null;
                    if (clientContext.Web.AllProperties.FieldValues.ContainsKey(Core.Constants.G1BuiltInProperties.SiteTemplateId))
                    {
                        siteTemplateId = clientContext.Web.AllProperties[Core.Constants.G1BuiltInProperties.SiteTemplateId].ToString()?.ToLower();
                    }

                    bool? isPublic = null;
                    if (clientContext.Web.AllProperties.FieldValues.ContainsKey(Core.Constants.G1BuiltInProperties.IsPublic))
                    {
                        isPublic = bool.Parse(clientContext.Web.AllProperties[Core.Constants.G1BuiltInProperties.IsPublic].ToString()?.ToLower());
                    }

                    var siteProperties = SitesService.ExtractSitePropertiesFromG1(siteDirectoryInfo, MigrationSettings.Value.WCMContextSettings);

                    var adminUsers = new List<string>();
                    foreach (var user in owners)
                    {
                        if (user.LoginName == "SHAREPOINT\\system")
                            continue;

                        adminUsers.Add(user.LoginName.Split('|').Last());
                    }
                    //remove group membership in owner list
                    //foreach(var user in adminUsers)
                    //{
                    //    if(user.EndsWith("_o"))
                    //    {
                    //        adminUsers.Remove(user);
                    //        if (adminUsers.Count() == 0)
                    //        {
                    //            break;
                    //        }

                    //    }
                    //}
                    for (int i = 0; i < adminUsers.Count; i++)
                    {
                        if (adminUsers[i].EndsWith("_o"))
                        {
                            adminUsers.Remove(adminUsers[i]);                       
                        }
                    }
                    SiteMigrationItem siteMigrationItem = new SiteMigrationItem
                    {
                        SiteUrl = siteUrl,
                        Title = clientContext.Web.Title,
                        Description = clientContext.Web.Description,
                        LCID = clientContext.Web.Language,
                        TimeZoneId = clientContext.Web.RegionalSettings.TimeZone.Id,
                        GroupId = groupId,
                        GroupAlias = groupAlias,
                        GroupType = groupType,
                        G1SiteTemplateId = siteTemplateId,
                        PermissionIdentities = new Fx.Models.Apps.AppInstanceIdentities
                        {
                            //Hieu rem
                            Admin =  new List<Fx.Models.Identities.Identity>() { 
                             
                            } 
                            //adminUsers
                        },
                        EnterpriseProperties = siteProperties,
                        SPTemplate = $"{clientContext.Web.WebTemplate}#{clientContext.Web.Configuration}",
                        IsPublic = isPublic
                    };
                    SiteTemplateMapping siteTemplateMapping = SitesService.GetSiteTemplateMapping(siteTemplateMappings, siteMigrationItem);
                    if (siteTemplateMapping != null)
                    {
                        ExportSitesReport.Instance.ModernSites.Add(siteMigrationItem);
                    }
                }
                catch (Exception exception)
                {
                    ExportSitesReport.Instance.AddFailedSite(siteUrl, exception);
                }
            }
        }

        private async Task<List<string>> QueryListOfSites()
        {
            var sitesToMigrates = new List<string>();
            sitesToMigrates.AddRange(await SitesService.GetG1SPFxSitesAsync());
            sitesToMigrates.AddRange(await SitesService.GetG1MasterPageSitesAsync());
            sitesToMigrates.AddRange(await SitesService.GetSitesWithG1Feature(Core.Constants.G1FeatureIds.TeamSitePrerequisites));
            sitesToMigrates = sitesToMigrates.Distinct().ToList();
            return sitesToMigrates;
        }
    }
}

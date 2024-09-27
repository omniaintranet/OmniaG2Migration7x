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
using static Omnia.Fx.Constants.Parameters;
using DocumentFormat.OpenXml.Spreadsheet;
using Omnia.Workplace.Models;
using Omnia.Workplace.Models.TeamWork.TeamWorkApp;
using DocumentFormat.OpenXml.Bibliography;
using System.Security.Policy;
using Microsoft.IdentityModel.Tokens;
using Omnia.Fx.SharePoint.CamlexNET;
using DocumentFormat.OpenXml.Wordprocessing;
using Omnia.Workplace;
using Microsoft.Office.SharePoint.Tools;
using Omnia.Fx.Apps;
using Omnia.Fx.Models.Shared;
using AngleSharp.Common;
using Omnia.Fx.Models.Extensions;
using Omnia.Fx.SharePoint.Fields.BuiltIn;

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
        private FeatureApiHttpClient featureApiHttpClientG2 { get; }
        private SPTokenService SPTokenService { get; }
        private SitesService SitesService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<ImportSitesAction> Logger { get; }

        private IProgressManager ProgressManager { get; set; }
        private IdentityApiHttpClient IdentityApiHttpClient { get; }
        private UserService UserService { get; }

        public ItemQueryResult<IResolvedIdentity> Identities { get; set; }
        private List<Omnia.Fx.Models.Apps.AppInstance> ExistingAppInstances { get; set; }
        private List<string> selectedPersonProperties { get; set; }
        private string actionFlag { get; set; }
        private string emaildefault { get; set; }
        public ImportSitesAction(
            AppApiHttpClient appApiHttpClient,
            G1FeatureApiHttpClient featureApiHttpClient,
            IdentityApiHttpClient identityApiHttpClient,
            FeatureApiHttpClient featureApiHttpClientG2,
            SPTokenService spTokenService,
              UserService userService,
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
            UserService = userService;
        }
      
        private List<string> Getuserbyemail(Fx.Models.Shared.ApiResponse<ItemQueryResult<IResolvedIdentity>> Identities, string email)
        {
            var result = new List<string>();

            foreach (var item in Identities.Data.Items)

            {
                if (item is ResolvedUserIdentity tuser)
                {

                    if (item.Type == IdentityTypes.User)
                    {
                        if (tuser.Email != null)
                        {


                            if (tuser.Email.Value.Email.ToString().ToLower() == email.ToLower())
                            {

                                string user = item.ToString();
                                result.Add(user);


                            }
                        }
                    }
                }
            }

            return result;


        }
        public override async Task StartAsync(IProgressManager progressManager)
        {
            Console.WriteLine("Team site migration........");
            //Console.WriteLine("   Select action:....");
            //Console.WriteLine("		1. Import Site...");
            //Console.WriteLine("     2. Get Test sites...");
            //string action = Console.ReadLine();
            //Console.WriteLine("Input default user email");

            //3
            //emaildefault= Console.ReadLine();


            Console.WriteLine("     1. Full Import...");
            Console.WriteLine("     2. Import site by filter file...");
            Console.WriteLine("     3. Re-activate all Feature...");



            string filterOption = Console.ReadLine();

            if (filterOption == "3")

            {
                //Console.WriteLine("Input Businessprofile ID");
                //string profileid = Console.ReadLine();
                // await LoadExistingAppInstancesAsync();
                var bplist = await AppApiHttpClient.GetBusinessProfilesAsync();
                var idlist = new List<string>();
                foreach (var profile in bplist.Data)
                {
                    idlist.Add(profile.Id.ToString());
                }
                await LoadallappAsync(idlist);
                await ReactivateallFeatures(ExistingAppInstances);

                Console.WriteLine("Done: The features is re-activated");
            }

            Console.WriteLine("   Delete app instance with URL NULL? Y/N...    ");
            actionFlag = Console.ReadLine();
            //Hieu added
           // var users = await IdentityApiHttpClient.GetUserIdentitybyEmail("");
            this.Identities = await UserService.LoadUserIdentity();

            
            //<<

            ProgressManager = progressManager;
            List<SiteMigrationItem> input = await ReadInput();
            string[] sitesfeature = ReadInputFeatures();



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

        private async Task LoadallappAsync(List<string> businessProfiles)
        {
            ExistingAppInstances = new List<Omnia.Fx.Models.Apps.AppInstance>();

            foreach (var profile in businessProfiles)
            {
                var appInstancesResult = await AppApiHttpClient.GetAppInstancesAsync(Core.Constants.AppDefinitionIDs.TeamCollaborationDefinitionID, new Guid(profile));
                appInstancesResult.EnsureSuccessCode();

                //ExistingAppInstances.AddRange(appInstancesResult.Data.AppInstances.Where(x => x.Status == Fx.Models.Apps.AppInstanceStatus.Ready).ToList());
                ExistingAppInstances.AddRange(appInstancesResult.Data.AppInstances.ToList());
            }
        }





        private async Task ReactivateallFeatures(List<Omnia.Fx.Models.Apps.AppInstance> apps)
        {
            foreach (var app in apps)
            {
                foreach (var featutureid in app.AppTemplate.Features)
                {
                    await AppApiHttpClient.FeatureReActivateAsync(featutureid.ToString(), app.Id.ToString());
                }

            }
        }






        private async Task FeatuteReActivate(string spUrl, string featureID, List<Omnia.Fx.Models.Apps.AppInstance> apps)
        {
            var app = apps.Where(a => a.DefaultResourceUrl.ToString() == spUrl).ToList().FirstOrDefault();
            string appid = app.Id.ToString();

            await AppApiHttpClient.FeatureReActivateAsync(featureID, appid);
            // Console.WriteLine(app.DefaultResourceUrl.ToString() + " " + feature.ToString());

        }
        private async Task SiteFeatuteReActivate(string[] spUrl, List<string> featureIDs, List<Omnia.Fx.Models.Apps.AppInstance> apps)
        {
            foreach (var sp in spUrl)
            {
                foreach (var id in featureIDs)
                {
                    await FeatuteReActivate(sp, id, apps);

                }
                Console.WriteLine("Done " + sp);
            }

        }

        private string[] ReadInputFeatures()
        {
            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.AppInstanceSettings.InputFile);
            string[] input = (System.IO.File.ReadAllLines(inputPath));

            return input;
        }
        private async ValueTask<List<SiteMigrationItem>> ReadInput()
        {

            var defaul = await IdentityApiHttpClient.GetUserIdentitybyEmail(emaildefault);

            var currentUser = UserMaper.GetIdentitybyEmail(this.Identities,emaildefault);

            //Getuserbyemail

            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportSitesSettings.InputFile);
            var input = JsonConvert.DeserializeObject<List<SiteMigrationItem1>>(System.IO.File.ReadAllText(inputPath));
            var input1 = new List<SiteMigrationItem>();
            foreach (var item in input)
            {

                var item1 = new SiteMigrationItem();
                item1.SiteUrl = item.SiteUrl;
                item1.Title = item.Title;
                item1.Description = item.Description;
                item1.LCID = item.LCID;
                item1.TimeZoneId = item.TimeZoneId;
                item1.GroupAlias = item.GroupAlias;
                item1.GroupId = item.GroupId;
                item1.GroupType = item.GroupType;
                item1.FeatureProperties = item.FeatureProperties;
                item1.EnterpriseProperties = item.EnterpriseProperties;
                item1.SPTemplate = item.SPTemplate;
                item1.IsPublic = item.IsPublic;
                item1.G1SiteTemplateId = item.G1SiteTemplateId;



                var appIdentities = new AppInstanceIdentities();
                var listAdmins = new List<Identity>();
                
                listAdmins.Add(currentUser);

                if (item.PermissionIdentities.Admin.Count > 0)
                {

                    foreach (var email in item.PermissionIdentities.Admin)

                    {

                        var adminuser  = UserMaper.GetIdentitybyEmail(this.Identities, email);

                        
                           if (adminuser != null)
                            listAdmins.Add(adminuser);
                        
                    }

                }

                appIdentities.Admin = listAdmins;
                item1.PermissionIdentities = appIdentities;
                input1.Add(item1);

            }



            if (MigrationSettings.Value.ImportSitesSettings.G1TemplatesToMigrate.Count > 0)
            {
                input1 = input1.Where(site => MigrationSettings.Value.ImportSitesSettings.G1TemplatesToMigrate.Any(x => x.ToLower() == site.G1SiteTemplateId?.ToLower())).ToList();
            }
            //Thoan
            return input1;

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

                    if (actionFlag != "Y")
                    {
                        //Hieu updated
                        //await SitesService.EnsureFailedUser(clientContext, site, selectedPersonProperties, Identities);
                    }
                    //Hieu rem
                    //var omniaCustomAction = await SitesService.GetOmniaG2CustomActionAsync(clientContext);
                    //Hieu added to check if site had already attached
                    var alreadyAttached = ExistingAppInstances.Where(s => s.DefaultResourceUrl != null && (s.DefaultResourceUrl.Equals(site.SiteUrl) || s.DefaultResourceUrl.Contains(site.SiteUrl))).ToList();
                    var canImportSite = true;
                    




                    //Hieu replace with this condition
                    if (alreadyAttached.Count == 1)
                    {
                        ImportSitesReport.Instance.AddSiteAlreadyAttached(site);
                        canImportSite = false;
                    }
                    if (siteTemplateMapping == null)
                    {
                        ImportSitesReport.Instance.AddSiteWithoutTemplate(site);
                        canImportSite = false;
                    }


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
                    Dictionary<string, object> g2EnterpriseProps = await MapEnterpriseProperties(site, siteTemplateMapping);
                    var keys = g2EnterpriseProps.Keys.ToList();
                    Dictionary<string, JToken> tokenDict = new Dictionary<string, JToken>();
                    keys.ForEach(k =>
                    {
                        tokenDict[k] = JToken.FromObject(g2EnterpriseProps[k]);
                    });



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
                        // Properties = createSiteProperties,
                        FeatureProperties = site.FeatureProperties,
                        EnterpriseProperties = g2EnterpriseProps,
                        PermissionIdentities = site.PermissionIdentities
                    };
                    //>>Hieu added

                    var appTitle = new MultilingualString();
                    appTitle.Add(defaultLang, site.Title);
                    var appDescription = new MultilingualString();
                    appDescription.Add(defaultLang, site.Description);

                    appInstanceInfo.Title = appTitle;
                    appInstanceInfo.Description = appDescription;




                    // từ G2 code

                    var properties = new TeamWorkAppInstanceProperties()
                    {
                        IsSiteAttached = true,

                        Permissions = new Permissions(),
                        SPPath = UrlHelper.GetRelativeUrl(site.SiteUrl),
                        SPAlias = UrlHelper.GetRelativeUrl(site.SiteUrl).Split("/").Last(),
                        Owner = site.PermissionIdentities.Admin[0],
                        Members = new List<Identity>(),
                        SiteDesignId = null,
                        OmniaPath = UrlHelper.GetRelativeUrl(site.SiteUrl).Split("/").Last(),
                       // OmniaRoutePrefix = Workplace.Fx.Constants.TeamWork.AppRoutePrefix,
                        Lcid = site.LCID,
                        TimezoneId = site.TimeZoneId
                    };

                    var appInstanceInputInfoG2 = new AppInstanceInputInfo
                    {
                        Title = appTitle,
                        Description = appDescription,
                        EnterpriseProperties = tokenDict ?? new Dictionary<string, JToken>(),
                        PermissionIdentities = site.PermissionIdentities,
                        Properties = properties,
                        FeatureProperties = new Dictionary<Guid, JObject>(),
                        ShowInPublicListings = ShowInPublicListingsMode.PublicToProfileViewer,
                        PendingRequestUrl = null
                    };
                    var IappInstanceIdentities = new AppInstanceIdentities
                    {

                    };

                    var newAppInstancePropertiesStorage = new AppInstancePropertiesStorage
                    {
                        OmniaPath = UrlHelper.GetRelativeUrl(site.SiteUrl),
                        
                    };




                    var appInstanceInputInfo7X = new AppInstanceInputInfo
                    {
                        Title = appTitle,
                        Description = appDescription,
                        ShowInPublicListings = ShowInPublicListingsMode.PublicToProfileViewer,
                        PermissionIdentities = site.PermissionIdentities,
                        Properties = newAppInstancePropertiesStorage
                    };





                    if (MigrationSettings.Value.ImportSitesSettings.UpdateSite)
                    {
                        if (alreadyAttached.Count > 0)
                        {
                            if (actionFlag != "Y" && actionFlag.IsNotNull())
                            {
                                var updateAppInstanceResult = await AppApiHttpClient.UpdateAppInstanceEnterprisePropertiesAsync1(
                                appInstanceId: alreadyAttached[0].Id,
                                profileId: siteTemplateMapping.BusinessProfileId,
                                spUrl: site.SiteUrl,
                                //appInstanceProperties: appInstanceInfo.EnterpriseProperties);
                                appInstanceProperties: appInstanceInputInfoG2);

                                updateAppInstanceResult.EnsureSuccessCode();
                                ImportSitesReport.Instance.AddSucceedSite(site);
                            }
                            //else if (actionFlag == "Y")
                            //{
                            //	//TODO: Add to report
                            //	var deleteAppInstanceResult = await AppApiHttpClient.DeleteAppInstanceAsync(appInstanceId: alreadyAttached[0].Id,
                            //	profileId: siteTemplateMapping.BusinessProfileId);

                            //	deleteAppInstanceResult.EnsureSuccessCode();
                            //	ImportSitesReport.Instance.AddSucceedSite(site);
                            //}
                            else
                            {
                                //TODO: Add to report
                            }
                        }
                    }
                    else
                    {

                        if (canImportSite)
                        {



                            var createAppInstanceResult = await AppApiHttpClient.CreateAppInstanceAsync(
                                   profileId: siteTemplateMapping.BusinessProfileId,
                                   appTemplateId: siteTemplateMapping.G2TemplateId,
                                   siteUrl: site.SiteUrl,
                                   inputInfo: appInstanceInputInfoG2);


                            createAppInstanceResult.EnsureSuccessCode();

                            //await SitesService.RemoveG1SPFxHeaderAsync(clientContext);
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

        private async ValueTask<Dictionary<string, object>> MapEnterpriseProperties(SiteMigrationItem site, SiteTemplateMapping siteTemplateMapping)
        {
            var g2EnterpriseProps = new Dictionary<string, object>();
            foreach (var g1Prop in site.EnterpriseProperties)
            {
                if (siteTemplateMapping.Properties.ContainsKey(g1Prop.Key) && g1Prop.Value != null)
                {
                    var oldValue = JToken.FromObject(g1Prop.Value);
                    var propMapping = siteTemplateMapping.Properties[g1Prop.Key];

                    //Diem - 30Jun2022: if user field value type - Recheck Quan
                    if (propMapping.PropertyType == Models.EnterpriseProperties.EnterprisePropertyType.User)
                    {
                        try
                        {
                            
                            var listuser = new List<object>();
                            //thoan sửa
                            foreach (var item in oldValue)
                            {
                               
                                var user = UserMaper.GetIdentitybyEmail(this.Identities,item.ToString());

                                if (user != null)
                                {

                                    listuser.Add( (UserIdentity)(user));

                                }
                            }

                            oldValue = JToken.FromObject(listuser);
                        }
                        catch
                        {
                            //nothing to do
                            oldValue = JToken.FromObject(g1Prop.Value);
                        }
                    }
                    if (propMapping.PropertyType == Models.EnterpriseProperties.EnterprisePropertyType.Taxonomy)
                    {
                        try
                        {

                            var listterm = new List<string>();
                            //thoan sửa
                            if (oldValue.Count() > 0)
                            {
                                foreach (var item in oldValue)
                                {
                                    listterm.Add(item["TermGuid"].ToString());

                                }
                            }

                            oldValue = JToken.FromObject(listterm);
                        }
                        catch
                        {
                            //nothing to do
                            oldValue = JToken.FromObject(g1Prop.Value);
                        }

                    }

                    g2EnterpriseProps.Add(propMapping.PropertyName, oldValue);
                }
            }

            return g2EnterpriseProps;
        }

        private void GetTestSiteOnly(List<SiteMigrationItem> sites)
        {
            List<string> listSite = new List<string>();
            foreach (var site in sites)
            {
                if (site.SiteUrl.Contains("test"))
                {
                    listSite.Add(site.SiteUrl);
                }
            }
            System.IO.File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "QueyTestSite.json"), JsonConvert.SerializeObject(listSite, Formatting.Indented));
        }
    }
}

using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.Links;
using Omnia.Migration.Models.Links;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Omnia.Migration.Actions
{
    public class ImportSharedLinksAction : BaseMigrationAction
    {
        private SharedLinkApiHttpClient SharedLinkApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<ImportSharedLinksAction> Logger { get; }
        private List<QuickLink> allExistingLinks { get; set; }
        public ImportSharedLinksAction(
            SharedLinkApiHttpClient sharedLinkApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            ILogger<ImportSharedLinksAction> logger)
        {
            SharedLinkApiHttpClient = sharedLinkApiHttpClient;
            MigrationSettings = migrationSettings;
            Logger = logger;
        }

        public override async Task StartAsync(IProgressManager progressManager)
        {

            ImportLinksReport.Instance.Init(MigrationSettings.Value);
            
            //08082022 - Diem: delete all links created by the "CreatedByUser" in appsetting before importing again.
            await DeleteOldLinksAsync(MigrationSettings.Value.ImportLinksSettings.CreatedByUser);

            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportLinksSettings.InputFile);
            var links = JsonConvert.DeserializeObject<List<G1CommonLink>>(File.ReadAllText(inputPath));
            progressManager.Start(links.Count);
            try
            {
                foreach (var link in links)
                {
                    if (link.Title.IsNull() && link.Url.IsNull())
                    {
                        ImportLinksReport.Instance.AddLinkWithoutURL(link.LinkId.ToString());
                        continue;
                    }
                    try
                    {
                        var newLink = LinkMapper.MapSharedLink(link, MigrationSettings.Value.WCMContextSettings, MigrationSettings.Value.ImportLinksSettings.IconColor, MigrationSettings.Value.ImportLinksSettings.BackgroundColor);
                        var addLinkResult = await SharedLinkApiHttpClient.AddOrUpdateSharedLink(newLink);
                        addLinkResult.EnsureSuccessCode();
                        ImportLinksReport.Instance.AddSucceedLink(link.Url);
                    }
                    catch (Exception ex)
                    {
                        ImportLinksReport.Instance.AddFailedLink(link.Url, ex);
                    }

                    progressManager.ReportProgress(1);
                }
            }
            catch (Exception ex)
            {
                ImportLinksReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
                //Logger.LogError(ex.Message + ex.StackTrace);
                Omnia.Migration.Core.Helpers.Logger.Log(ex.Message + ex.StackTrace);
            }
            finally
            {
                ImportLinksReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
            }
        }
        private async Task LoadExistingLinksAsync()
        {
            allExistingLinks = new List<QuickLink>();
            var apiResponse = await SharedLinkApiHttpClient.GetAllSharedLinks();
            apiResponse.EnsureSuccessCode();
            allExistingLinks.AddRange(apiResponse.Data);
        }
        private async Task DeleteOldLinksAsync(string createdBy)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WorkplaceContextSettings.DatabaseConnectionString))
            {
                await connection.ExecuteAsync(@"
                    Delete SharedLinks                    
                    WHERE CreatedBy = @Createdby", new { Createdby = createdBy });
            }
        }
    }
}

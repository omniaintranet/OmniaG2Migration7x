﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Core.Services;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.Links;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace Omnia.Migration.Actions
{
    public class ImportMyLinksAction : BaseMigrationAction
    {
        private LinksService LinksService { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        public ImportMyLinksAction(
            LinksService linksService,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            LinksService = linksService;
            MigrationSettings = migrationSettings;
        }
        public override async Task StartAsync(IProgressManager progressManager)
        {
            ImportLinksReport.Instance.Init(MigrationSettings.Value);

            //08082022 - Diem: delete all links created by the "CreatedByUser" in appsetting before importing again.
            await DeleteOldLinksAsync(MigrationSettings.Value.ImportMyLinksSettings.CreatedByUser);

            try
            {
                var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportMyLinksSettings.InputFile);

                var links = JsonConvert.DeserializeObject<List<G1MyLink>>(File.ReadAllText(inputPath));
                progressManager.Start(links.Count);

                foreach (var link in links)
                {
                    if (link.Title.IsNull() && link.Url.IsNull())
                    {
                        ImportLinksReport.Instance.AddLinkWithoutURL(link.LinkId.ToString());
                        continue;
                    }
                    await LinksService.AddOrUpdateMyLinkAsync(link);

                    progressManager.ReportProgress(1);
                }
            }
            catch (Exception ex)
            {
                ImportLinksReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
                //Logger.LogError(ex.Message + ex.StackTrace);
                Logger.Log(ex.Message + ex.StackTrace);
            }
            finally
            {
                ImportLinksReport.Instance.ExportTo(MigrationSettings.Value.OutputPath);
            }
        }
        private async Task DeleteOldLinksAsync(string createdBy)
        {
            using (var connection = new SqlConnection (MigrationSettings.Value.WorkplaceContextSettings.DatabaseConnectionString))
            {
                await connection.ExecuteAsync(@"
                    Delete MyLinks                    
                    WHERE CreatedBy = @Createdby", new { Createdby = createdBy });
            }
        }
    }
}

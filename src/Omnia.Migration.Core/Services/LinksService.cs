using Dapper;
using Microsoft.Extensions.Options;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.Links;
using Omnia.Migration.Models.Links;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Omnia.Migration.Core.Reports;

namespace Omnia.Migration.Core.Services
{
    public class LinksService

    {
        private MyLinkApiHttpClient MyLinkApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public LinksService(MyLinkApiHttpClient myLinksApiHttpClient,

            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            MyLinkApiHttpClient = myLinksApiHttpClient;
            MigrationSettings = migrationSettings;
        }

        public async ValueTask<QuickLink> AddOrUpdateMyLinkAsync(G1MyLink link)
        {
            
            try
            {

                var g2Link = LinkMapper.MapSharedLink(link, MigrationSettings.Value.WCMContextSettings, MigrationSettings.Value.ImportMyLinksSettings.IconColor, MigrationSettings.Value.ImportMyLinksSettings.BackgroundColor);
                var addLinkResult = await MyLinkApiHttpClient.AddOrUpdateMyLinkAsync(g2Link);
                addLinkResult.EnsureSuccessCode();

                using (var connection = new SqlConnection(MigrationSettings.Value.WorkplaceContextSettings.DatabaseConnectionString))
                {
                    await connection.ExecuteAsync(@"
                    Update MyLinks 
                    SET UserLoginName = @UserLoginName
                    WHERE Id = @Id", new { Id = addLinkResult.Data.Id, UserLoginName = link.UserLoginName });
                }

                ImportLinksReport.Instance.AddSucceedLink(link.Url);
                return addLinkResult.Data;
            }
            catch (Exception ex)
            {
                ImportLinksReport.Instance.AddFailedLink(link.Url, ex);
                return null;
            }


        }
    }
}

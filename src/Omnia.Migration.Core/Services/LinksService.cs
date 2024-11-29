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
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;

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

        public async ValueTask<QuickLink> AddOrUpdateMyLinkAsync(G1MyLink link, ItemQueryResult<IResolvedIdentity> Identities)
        {

            try
            {

                var g2Link = LinkMapper.MapSharedLink(link, MigrationSettings.Value.WCMContextSettings, MigrationSettings.Value.ImportMyLinksSettings.IconColor, MigrationSettings.Value.ImportMyLinksSettings.BackgroundColor);
                if (g2Link != null)
                {
                    var addLinkResult = await MyLinkApiHttpClient.AddOrUpdateMyLinkAsync(g2Link);
                    addLinkResult.EnsureSuccessCode();
                    var Icreadby1 = Omnia.Migration.Core.Mappers.UserMaper.GetSystemPropUserIdentitybyEmail(Identities, link.UserLoginName);
                    if (Icreadby1 != null)
                    {

                        using (var connection = new SqlConnection(MigrationSettings.Value.WorkplaceContextSettings.DatabaseConnectionString))
                        {
                            await connection.ExecuteAsync(@"
                    Update MyLinks 
                    SET UserLoginName = @UserLoginName
                    WHERE Id = @Id", new { Id = addLinkResult.Data.Id, UserLoginName = Icreadby1 });
                        }
                    }
                    ImportLinksReport.Instance.AddSucceedLink(link.Url);
                    return addLinkResult.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                ImportLinksReport.Instance.AddFailedLink(link.Url, ex);
                return null;
            }


        }
    }
}

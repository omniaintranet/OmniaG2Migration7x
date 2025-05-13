using Dapper;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
    public class PagesService
    {
        private NavigationApiHttpClient NavigationApiHttpClient { get; }
        private VariationApiHttpClient VariationApiHttpClient { get; }
        private PageApiHttpClient PageApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public PagesService(
            NavigationApiHttpClient navigationApiHttpClient,
            VariationApiHttpClient variationApiHttpClient,
            PageApiHttpClient pageApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            NavigationApiHttpClient = navigationApiHttpClient;
            VariationApiHttpClient = variationApiHttpClient;
            PageApiHttpClient = pageApiHttpClient;
            MigrationSettings = migrationSettings;
        }

        public void MergePageData(PageData src, PageData update)
        {
            src.ParentLayoutPageId = update.ParentLayoutPageId;
            if (MigrationSettings.Value.ImportPagesSettings.ImportPageContent)
            {
                foreach (var prop in update.EnterpriseProperties)
                {
                    src.EnterpriseProperties.AddOrUpdate(prop.Key, prop.Value);
                }
            }
            
            if (MigrationSettings.Value.ImportPagesSettings.ImportBlockSettings) 
            {
                foreach (var prop in update.PropertyBag)
                {
                    src.PropertyBag.AddOrUpdate(prop.Key, prop.Value);
                }

                foreach (var blockSetting in update.Layout.BlockSettings)
                {
                    src.Layout.BlockSettings.AddOrUpdate(blockSetting.Key, blockSetting.Value);
                }

                foreach (var layoutItem in update.Layout.Definition.Items)
                {
                    var existingLayoutItem = src.Layout.Definition.Items.FirstOrDefault(x => x.Id == layoutItem.Id);
                    if (existingLayoutItem != null)
                    {
                        src.Layout.Definition.Items.Remove(existingLayoutItem);
                    }
                    layoutItem.OwnerLayoutId = src.Layout.Definition.Id;
                    src.Layout.Definition.Items.Add(layoutItem);
                }
            }; 
        }
          
        public async Task UpdatePageSystemInfoAsync(int pageId, int versionId, PageNavigationMigrationItem page, ItemQueryResult<IResolvedIdentity> identities)
        {
            if (string.IsNullOrEmpty(page.CreatedAt) ||
                string.IsNullOrEmpty(page.CreatedBy) ||
                string.IsNullOrEmpty(page.ModifiedAt) ||
                string.IsNullOrEmpty(page.ModifiedBy))
                return;

            var ImodifiedBy = page.ModifiedBy;// UserMaper.GetSystemPropUserIdentitybyEmail(identities, page.ModifiedBy);
            var IcreatedBy = page.CreatedBy;// UserMaper.GetSystemPropUserIdentitybyEmail(identities, page.CreatedBy);
            if (string.IsNullOrEmpty(ImodifiedBy) || (string.IsNullOrEmpty(IcreatedBy)))
                return;


            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                //Hieu rem: need to update here
                // Thoan modified 7.6
                await connection.ExecuteAsync(@"
                    Update Pages 
                    SET CreatedBy = @CreatedBy, CreatedAt = @CreatedAt, ModifiedBy = @ModifiedBy, ModifiedAt = @ModifiedAt 
                    WHERE Id = @PageId", new { PageId = pageId, CreatedBy = IcreatedBy, CreatedAt = page.CreatedAt, ModifiedBy = ImodifiedBy, ModifiedAt = page.ModifiedAt });


                await connection.ExecuteAsync(@"
                    Update VersionedPageData 
                    SET CreatedBy = @CreatedBy, CreatedAt = @CreatedAt, ModifiedBy = @ModifiedBy, ModifiedAt = @ModifiedAt 
                    WHERE Id = @VersionId", new { VersionId = versionId, CreatedBy = IcreatedBy, CreatedAt = page.CreatedAt, ModifiedBy = ImodifiedBy, ModifiedAt = page.ModifiedAt });
            }
        }

        public async ValueTask<PageCollectionNavigationNode<PageCollectionNavigationData>> GetPageCollectionNodeAsync(int pageCollectionId)
        {
            var pageCollectionResult = await NavigationApiHttpClient.GetPageNavigationNodesAsync(new PageId[] { pageCollectionId });
            pageCollectionResult.EnsureSuccessCode();

            if (!pageCollectionResult.Data.ContainsKey(pageCollectionId) || pageCollectionResult.Data[pageCollectionId].Count == 0)
                throw new Exception("Cannot find page collection with ID " + pageCollectionId);

            return pageCollectionResult.Data[pageCollectionId][0] as PageCollectionNavigationNode<PageCollectionNavigationData>;
        }

        public async Task AddEventParticipantAsync(Guid eventId, EventParticipant participant, ItemQueryResult<IResolvedIdentity> identities)
        {
            if (string.IsNullOrEmpty(participant.CreatedAt) ||
                string.IsNullOrEmpty(participant.CreatedBy) ||
                string.IsNullOrEmpty(participant.ModifiedAt) ||
                string.IsNullOrEmpty(participant.ModifiedBy))
                return;

            var ImodifiedBy = UserMaper.GetSystemPropUserIdentitybyEmail(identities, participant.ModifiedBy);
            var IcreatedBy = UserMaper.GetSystemPropUserIdentitybyEmail(identities, participant.CreatedBy);
            if (string.IsNullOrEmpty(ImodifiedBy) || (string.IsNullOrEmpty(IcreatedBy)))
                return;

            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO Participants (Id,EventId,CreatedBy,ModifiedBy,CreatedAt,ModifiedAt,LoginName,Name,Email,Phone,Comment,Capacity,ParticipantType,StatusResponse,StatusTime,OutlookEventId)
                     VALUES (@Id,@EventId,@CreatedBy,@ModifiedBy,@CreatedAt,@ModifiedAt,@LoginName,@Name,@Email,@Phone,@Comment,@Capacity,@ParticipantType,@StatusResponse,@StatusTime,@OutlookEventId)",
                     new
                     {
                         Id = Guid.NewGuid(),
                         EventId = eventId.ToString(),
                         CreatedBy = IcreatedBy,
                         CreatedAt = participant.CreatedAt,
                         ModifiedBy = ImodifiedBy,
                         ModifiedAt = participant.ModifiedAt,
                         LoginName = participant.LoginName,
                         Name = participant.Name,
                         Email = participant.Email,
                         Phone = participant.Phone,
                         Comment = participant.Comment,
                         Capacity = participant.Capacity,
                         ParticipantType = participant.ParticipantType,
                         StatusResponse = participant.StatusResponse,
                         StatusTime = participant.StatusTime,
                         OutlookEventId = participant.OutlookEventId
                     }
                     );
            }
        }

        public async Task UpdateEventDetailsAsync(Guid eventId, int registeredCapacity, string outlookEventId)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                await connection.ExecuteAsync(@"
                    UPDATE Events Set RegisteredCapacity = @RegisteredCapacity, OutlookEventId = @OutlookEventId Where Id = @Id",
                     new
                     {
                         Id = eventId,
                         RegisteredCapacity = registeredCapacity,
                         OutlookEventId = outlookEventId
                     }
                     );
            }
        }
    }
}

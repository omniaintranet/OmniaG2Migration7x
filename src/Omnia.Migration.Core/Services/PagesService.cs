using Dapper;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Omnia.Migration.Core.Mappers;

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
          
        public async Task UpdatePageSystemInfoAsync(int pageId, int versionId, PageNavigationMigrationItem page, ItemQueryResult<IResolvedIdentity> Identities)
        {
            if (string.IsNullOrEmpty(page.CreatedAt) ||
                string.IsNullOrEmpty(page.CreatedBy) ||
                string.IsNullOrEmpty(page.ModifiedAt) ||
                string.IsNullOrEmpty(page.ModifiedBy))
                return;

            var ImodifiedBy = UserMaper.GetSystemPropUserIdentitybyEmail(Identities, page.ModifiedBy);
            var IcreatedBy= UserMaper.GetSystemPropUserIdentitybyEmail(Identities, page.CreatedBy);
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
    }
}

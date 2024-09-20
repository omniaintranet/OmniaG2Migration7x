using Dapper;
using Microsoft.Extensions.Options;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Social;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Omnia.Fx.Models.Social;
using System.ComponentModel.Design;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Workplace.Models.Social;
using DocumentFormat.OpenXml.Vml;

namespace Omnia.Migration.Core.Services
{
    public class SocialService
    {
        private SocialApiHttpClient SocialApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public SocialService(
            SocialApiHttpClient socialApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            SocialApiHttpClient = socialApiHttpClient;
            MigrationSettings = migrationSettings;
        }

        public async Task ImportCommentsAndLikesAsync(PageId pageId, PageNavigationMigrationItem migrationItem, PageNavigationNode<PageNavigationData> existingPage, ItemQueryResult<IResolvedIdentity> Identities)
        {
            if (MigrationSettings.Value.ImportPagesSettings.ImportLikesAndComments)
            {
                if (existingPage != null)
                    await DeleteOldCommentsAndLikesIfNeededAsync((int)pageId, migrationItem);

                var commentIdMap = new Dictionary<Guid, Guid>();

                foreach (var comment in migrationItem.Comments)
                {
                    var newComment = await ImportCommentAsync(pageId, null, comment, Identities);
                    commentIdMap.Add(comment.Id, newComment.Id);
                }

                foreach (var like in migrationItem.Likes)
                {
                    await DBAddLike(pageId, like, Identities, "");
                }
                //await UpdateDateLikes((int)pageId, migrationItem, Identities);
            }
        }

        private async ValueTask<Omnia.Fx.Models.Social.Comment> ImportCommentAsync(PageId pageId, Guid? parentId, G1Comment comment, ItemQueryResult<IResolvedIdentity> Identities)
        {

            var newComment = SocialMapper.MapComment(pageId, parentId, comment, Identities);
            if (newComment.CreatedBy != null)
            {
                var addCommentResult = await SocialApiHttpClient.AddComment(newComment);
                addCommentResult.EnsureSuccessCode();
                await UpdateComments(Identities, addCommentResult.Data, comment);

                if (comment.Likes != null && comment.Likes.Count > 0)
                {
                    foreach (var like in comment.Likes)
                    {
                        // await ImportLikeAsync(pageId, addCommentResult.Data.Id, like);

                        await DBAddLike(pageId, like, Identities, addCommentResult.Data.Id.ToString());
                    }
                }

                if (comment.Children != null && comment.Children.Count > 0)
                {
                    foreach (var childComment in comment.Children)
                    {
                        await ImportCommentAsync(pageId, addCommentResult.Data.Id, childComment, Identities);
                    }
                }

                return addCommentResult.Data;
            }
            return null;
        }

        private async Task ImportLikeAsync(PageId pageId, Guid? commentId, G1Like like)
        {
            string topicId = WebContentManagement.Fx.Constants.TopicPrefixes.Page + pageId;
            var addLikeResult = await SocialApiHttpClient.AddOrUpdateLike(topicId, commentId?.ToString(), true, like.CreatedBy);

            addLikeResult.EnsureSuccessCode();
        }

        private async Task DeleteOldCommentsAndLikesIfNeededAsync(int pageId, PageNavigationMigrationItem migrationItem)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();

                if (migrationItem.Comments.Count > 0)
                {
                    await connection.ExecuteAsync(@"
                        Update Comments SET DeletedAt = GETDATE(), DeletedBy = @ClientId WHERE TopicId='page-' + @PageId", new { PageId = pageId.ToString(), ClientId = clientId });
                }

                if (migrationItem.Likes.Count > 0)
                {
                    await connection.ExecuteAsync(@"
                        Update LIKES SET DeletedAt = GETDATE() WHERE TopicId='page-' + @PageId", new { PageId = pageId.ToString() });
                }
            }
        }
        private async Task UpdateDateLikes(int pageId, PageNavigationMigrationItem migrationItem, ItemQueryResult<IResolvedIdentity> Identities)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();
                if (migrationItem.Likes.Count > 0)
                {
                    foreach (var like in migrationItem.Likes)
                    {
                        var ICreatedby = GetIdentitybyEmail(Identities, like.CreatedBy);
                        string Iuser = ICreatedby.Id.ToString() + "[1]";
                        if (like.CommentId != "")
                        {

                            await connection.ExecuteAsync(@"
                        Update LIKES SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt WHERE TopicId='page-' + @PageId AND CreatedBy=@CreatedBy AND CommentId=@CommentId", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = Iuser, CommentId = like.CommentId });
                        }
                        else
                        {
                            await connection.ExecuteAsync(@"
                        Update LIKES SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt WHERE TopicId='page-' + @PageId AND CreatedBy=@CreatedBy", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = Iuser });
                        }
                    }
                }
            }
        }
        private async Task DBAddLike(int pageId, G1Like like, ItemQueryResult<IResolvedIdentity> Identities, string commentID)
        {
            var ICreatedby = GetIdentitybyEmail(Identities, like.CreatedBy);
            string Iuser = ICreatedby.Id.ToString() + "[1]";
            if (ICreatedby != null)
            {

                using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
                {

                    await connection.ExecuteAsync(@"
                        INSERT INTO  LIKES (CreatedBy,ModifiedBy,CreatedAt,ModifiedAt,CommentId,TopicId,ReactionType) VALUES (@CreatedBy,@ModifiedBy,@CreatedAt,@ModifiedAt,@CommentId,'page-' + @PageId,'1' )", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = Iuser, CommentId = commentID, ModifiedBy = Iuser });





                }
            }
        }


        private async Task UpdateComments(ItemQueryResult<IResolvedIdentity> Identities, Omnia.Fx.Models.Social.Comment newcomment, G1Comment comment)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();

                var ICreatedby = GetIdentitybyEmail(Identities, comment.CreatedBy);
                string Iuser = ICreatedby.Id.ToString() + "[1]";
                await connection.ExecuteAsync(@"
                        Update Comments SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt, ModifiedBy= @ModifiedBy, CreatedBy=@CreatedBy WHERE  ID=@ID"
                , new { CreatedAt = comment.CreatedAt, ModifiedAt = comment.ModifiedAt, CreatedBy = Iuser, ID = newcomment.Id, ModifiedBy = Iuser });

            }
        }
        private static Identity GetIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return (Identity)item;
                }
            }
            return null;
        }


    }
}

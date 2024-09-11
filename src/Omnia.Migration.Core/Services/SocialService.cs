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

        public async Task ImportCommentsAndLikesAsync(PageId pageId, PageNavigationMigrationItem migrationItem, PageNavigationNode<PageNavigationData> existingPage)
        {
            if (MigrationSettings.Value.ImportPagesSettings.ImportLikesAndComments)
            {
                if (existingPage != null)
                    await DeleteOldCommentsAndLikesIfNeededAsync((int)pageId, migrationItem);

                var commentIdMap = new Dictionary<Guid, Guid>();

                foreach (var comment in migrationItem.Comments)
                {
                    var newComment = await ImportCommentAsync(pageId, null, comment);
                    commentIdMap.Add(comment.Id, newComment.Id);
                }

                foreach (var like in migrationItem.Likes)
                {
                    Guid? commentId = !string.IsNullOrEmpty(like.CommentId) ? commentIdMap[new Guid(like.CommentId)] : (Guid?)null;
                    await ImportLikeAsync(pageId, commentId, like);
                }
                await UpdateDateLikes((int)pageId, migrationItem);
            }
        }

        private async ValueTask<Omnia.Fx.Models.Social.Comment> ImportCommentAsync(PageId pageId, Guid? parentId, G1Comment comment)
        {
            var newComment = SocialMapper.MapComment(pageId, parentId, comment);
            var addCommentResult = await SocialApiHttpClient.AddComment(newComment);
            addCommentResult.EnsureSuccessCode();

            if (comment.Likes != null && comment.Likes.Count > 0)
            {
                foreach (var like in comment.Likes)
                {
                    await ImportLikeAsync(pageId, addCommentResult.Data.Id, like);
                }
            }

            if (comment.Children != null && comment.Children.Count > 0)
            {
                foreach (var childComment in comment.Children)
                {
                    await ImportCommentAsync(pageId, addCommentResult.Data.Id, childComment);
                }
            }

            return addCommentResult.Data;
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
        private async Task UpdateDateLikes(int pageId, PageNavigationMigrationItem migrationItem)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();
                if (migrationItem.Likes.Count > 0)
                {
                    foreach (var like in migrationItem.Likes)
                    {
                        if (like.CommentId != "")
                        {
                            await connection.ExecuteAsync(@"
                        Update LIKES SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt WHERE TopicId='page-' + @PageId AND CreatedBy=@CreatedBy AND CommentId=@CommentId", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = like.CreatedBy, CommentId = like.CommentId });
                        }
                        else
                        {
                            await connection.ExecuteAsync(@"
                        Update LIKES SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt WHERE TopicId='page-' + @PageId AND CreatedBy=@CreatedBy", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = like.CreatedBy });
                        }
                    }
                }
            }
        }





    }
}

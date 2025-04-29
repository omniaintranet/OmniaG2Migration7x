using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
    public class SocialService
    {
        private ImagesService ImagesService { get; }
        private IHttpImageClient imageHttpClient { get; }
        private WcmImageApiHttpClient ImageApiHttpClient { get; }
        private SocialApiHttpClient SocialApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }

        public SocialService(
            ImagesService imagesService,
            SharePointImageHttpClient sharePointImageHttpClient,
            CustomHttpImageClient customHttpImageClient,
            WcmImageApiHttpClient imageApiHttpClient,
            SocialApiHttpClient socialApiHttpClient,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            ImagesService = imagesService;
            ImageApiHttpClient = imageApiHttpClient;
            SocialApiHttpClient = socialApiHttpClient;
            MigrationSettings = migrationSettings;
            if (MigrationSettings.Value.UseCustomImageClient)
            {
                imageHttpClient = customHttpImageClient;
            }
            else
            {
                imageHttpClient = sharePointImageHttpClient;
            }
        }

        public async Task ImportCommentsAndLikesAsync(PageId pageId, PageNavigationMigrationItem migrationItem, PageNavigationNode<PageNavigationData> existingPage, ItemQueryResult<IResolvedIdentity> identities)
        {
            if (MigrationSettings.Value.ImportPagesSettings.ImportLikesAndComments)
            { 
                if (existingPage != null)
                    //  await DeleteOldCommentsAndLikesIfNeededAsync((int)pageId, migrationItem);
                    await DeleteOldCommentsAndLikesAsync((int)pageId, migrationItem);

                //  var commentIdMap = new Dictionary<Guid, Guid>();
                var migratedImages = new Dictionary<string, string>();

                foreach (var comment in migrationItem.Comments)
                {
                    var newComment = await ImportCommentAsync(pageId, null, comment, identities, migratedImages);
                  //  commentIdMap.Add(comment.Id, newComment.Id);
                }

                foreach (var like in migrationItem.Likes)
                {
                    await DBAddLike(pageId, like, identities, "");
                }
                //await UpdateDateLikes((int)pageId, migrationItem, Identities);
            }
        }

        private async ValueTask<Omnia.Fx.Models.Social.Comment> ImportCommentAsync(PageId pageId, Guid? parentId, G1Comment comment, ItemQueryResult<IResolvedIdentity> identities, Dictionary<string, string> migratedImages)
        {
            if (MigrationSettings.Value.ImportPagesSettings.MigrateImages)
            {
                var commentContent = EnterprisePropertyMapper.MapTextPropertyValue(comment.Content, MigrationSettings.Value.WCMContextSettings).ToString();
                var sharepointUrl = MigrationSettings.Value.WCMContextSettings.SharePointUrl.ToLower();
                
                comment.Content = await MigrateCommentImages(commentContent, sharepointUrl, pageId, migratedImages);
            }

            var newComment = SocialMapper.MapComment(pageId, parentId, comment, identities);
            if (newComment.CreatedBy != null)
            {
                var addCommentResult = await SocialApiHttpClient.AddComment(newComment);
                addCommentResult.EnsureSuccessCode();
                await UpdateComments(identities, addCommentResult.Data, comment);

                if (comment.Likes != null && comment.Likes.Count > 0)
                {
                    foreach (var like in comment.Likes)
                    {
                        // await ImportLikeAsync(pageId, addCommentResult.Data.Id, like);
                        // Thoan modified 7.6
                        await DBAddLike(pageId, like, identities, addCommentResult.Data.Id.ToString());
                    }
                }

                if (comment.Children != null && comment.Children.Count > 0)
                {
                    foreach (var childComment in comment.Children)
                    {
                        await ImportCommentAsync(pageId, addCommentResult.Data.Id, childComment, identities, migratedImages);
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
        private async Task DeleteOldCommentsAndLikesAsync(int pageId, PageNavigationMigrationItem migrationItem)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();

                if (migrationItem.Comments.Count > 0)
                {
                    await connection.ExecuteAsync(@"
                        DELETE from Comments where TopicId='page-' + @PageId", new { PageId = pageId.ToString() });
                }

                if (migrationItem.Likes.Count > 0)
                {
                    await connection.ExecuteAsync(@"
                        DELETE from Likes where TopicId='page-' + @PageId", new { PageId = pageId.ToString() });
                }
            }
        }
        private async Task UpdateDateLikes(int pageId, PageNavigationMigrationItem migrationItem, ItemQueryResult<IResolvedIdentity> identities)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();
                if (migrationItem.Likes.Count > 0)
                {
                    foreach (var like in migrationItem.Likes)
                    {
                        var ICreatedby = GetIdentitybyEmail(identities, like.CreatedBy);
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
        // Thoan modified 7.6
        private async Task DBAddLike(int pageId, G1Like like, ItemQueryResult<IResolvedIdentity> identities, string commentID)
        {
            var ICreatedby = GetIdentitybyEmail(identities, like.CreatedBy);
          
            if (ICreatedby != null)
            {
                string Iuser = ICreatedby.Id.ToString() + "[1]";

                using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
                {

                    await connection.ExecuteAsync(@"
                        INSERT INTO  LIKES (CreatedBy,ModifiedBy,CreatedAt,ModifiedAt,CommentId,TopicId,ReactionType) VALUES (@CreatedBy,@ModifiedBy,@CreatedAt,@ModifiedAt,@CommentId,'page-' + @PageId,'1' )", new { PageId = pageId.ToString(), CreatedAt = like.CreatedAt, ModifiedAt = like.ModifiedAt, CreatedBy = Iuser, CommentId = commentID, ModifiedBy = Iuser });



                }
            }
        }


        private async Task UpdateComments(ItemQueryResult<IResolvedIdentity> identities, Omnia.Fx.Models.Social.Comment newcomment, G1Comment comment)
        {
            using (var connection = new SqlConnection(MigrationSettings.Value.WCMContextSettings.DatabaseConnectionString))
            {
                var clientId = MigrationSettings.Value.OmniaSecuritySettings.ClientId.ToString();

                var ICreatedby = GetIdentitybyEmail(identities, comment.CreatedBy);
                string Iuser = ICreatedby.Id.ToString() + "[1]";
                await connection.ExecuteAsync(@"
                        Update Comments SET CreatedAt = @CreatedAt, ModifiedAt = @ModifiedAt, ModifiedBy= @ModifiedBy, CreatedBy=@CreatedBy WHERE  ID=@ID"
                , new { CreatedAt = comment.CreatedAt, ModifiedAt = comment.ModifiedAt, CreatedBy = Iuser, ID = newcomment.Id, ModifiedBy = Iuser });

            }
        }
        private static Identity GetIdentitybyEmail(ItemQueryResult<IResolvedIdentity> identities, string email)
        {
            foreach (ResolvedUserIdentity item in identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return (Identity)item;
                }
            }
            return null;
        }

        private async Task<string> MigrateCommentImages(string commentContent, string sharepointUrl, int pageId, Dictionary<string, string> migratedImages)
        {
            var imageSrcs = HtmlParser.ParseAllImageUrls(commentContent);
            foreach (var imageSrc in imageSrcs)
            {
                try
                {
                    if (migratedImages.ContainsKey(imageSrc))
                    {
                        commentContent = commentContent.Replace(imageSrc, migratedImages[imageSrc]);
                    }
                    else if (imageSrc.ToLower().StartsWith(sharepointUrl))
                    {
                        var imageFileName = System.IO.Path.GetFileName(imageSrc).Split("?")[0];
                        var imageContent = await imageHttpClient.GetImage(imageSrc);
                        var size = imageContent.Length / 1024;
                        while (size > 10000)
                        {
                            //Reduce image size                            
                            imageContent = ImagesService.ReduceImageSize(imageContent);
                            size = imageContent.Length / 1024;
                        }
                        var base64 = Convert.ToBase64String(imageContent);
                        try
                        {
                            var svgContent = JsonConvert.DeserializeObject<Models.BlockData.SVGViewerData>(commentContent);
                            if (svgContent.documentUrl != null)
                            {
                                var imgParts = imageSrc.Split("/");
                                string imgPath = imgParts[0] + "//" + imgParts[2] + "/" + imgParts[3] + "/" + imgParts[4];
                                svgContent.name = imageFileName.Split(".svg").First();
                                svgContent.spWebUrl = imgPath;
                                commentContent = JsonConvert.SerializeObject(svgContent);
                                continue;
                            }
                        }
                        catch { }

                        var newImageSrcResult = await ImageApiHttpClient.UploadPageImageAsync(base64, pageId, imageFileName, isCommentImage: true);
                        File.WriteAllBytes(imageFileName, imageContent);
                        using (var stream = new MemoryStream(File.ReadAllBytes(imageFileName)))
                        {
                            try
                            {
                                var imgObj = Image.FromStream(stream, false, false);
                                var gcd = CommonUtils.GCD(imgObj.Width, imgObj.Height);

                                var xRatio = (imgObj.Width / gcd).ToString();
                                var yRatio = (imgObj.Height / gcd).ToString();
                                commentContent = commentContent.Replace("\"x\": 16.0", "\"x\": " + xRatio);
                                commentContent = commentContent.Replace("\"y\": 9.0", "\"y\": " + yRatio);
                                commentContent = commentContent.Replace("\"ratioDisplayName\": \"Custom\",\r\n      \"xRatio\": 16,\r\n      \"yRatio\": 9", "\"ratioDisplayName\": \"Custom\",\r\n      \"xRatio\": " + xRatio + ",\r\n      \"yRatio\": " + yRatio);
                            }
                            catch (Exception)
                            {
                            }

                        }
                        File.Delete(imageFileName);

                        commentContent = commentContent.Replace(imageSrc, newImageSrcResult);

                        var configParamStartIndex = commentContent.IndexOf("%22%2c%22configuration%22");
                        var configParamEndIndex = commentContent.IndexOf("Video%22%3afalse%7d");
                        if (configParamStartIndex > -1 && configParamEndIndex > configParamStartIndex)
                        {
                            var configParam = commentContent.Substring(configParamStartIndex, configParamEndIndex - configParamStartIndex + "Video%22%3afalse%7d".Length);
                            commentContent = commentContent.Replace(configParam, string.Empty);
                        }

                        migratedImages.Add(imageSrc, newImageSrcResult);
                    }
                }
                catch { }
            }

            return commentContent;
        }
    }
}

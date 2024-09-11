using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Social;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Mappers
{
    public static class SocialMapper
    {

        public static Omnia.Fx.Models.Social.Comment MapComment(int pageId, Guid? parentId, G1Comment comment)
        {
            return new Omnia.Fx.Models.Social.Comment()
            {
                Content = comment.Content,
                CreatedBy = comment.CreatedBy,
                CreatedAt = comment.CreatedAt,
                ModifiedAt = comment.ModifiedAt,
                TopicId = Omnia.WebContentManagement.Fx.Constants.TopicPrefixes.Page + pageId,
                ParentId = parentId
            };
        }
        public static Omnia.Fx.Models.Social.Like MapLike(int pageId, G1Like like)
        {
            return new Omnia.Fx.Models.Social.Like()
            {
                CommentId = like.CommentId,
                CreatedBy = like.CreatedBy,
                CreatedAt = like.CreatedAt,
                ModifiedBy = like.ModifiedBy,
                ModifiedAt = like.ModifiedAt,
                TopicId = Omnia.WebContentManagement.Fx.Constants.TopicPrefixes.Page + pageId
            };
        }
    }

}

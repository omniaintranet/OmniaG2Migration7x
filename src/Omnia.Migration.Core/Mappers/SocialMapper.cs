using Microsoft.Office.SharePoint.Tools;
using Microsoft.SharePoint.Client;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omnia.Migration.Core.Services;

namespace Omnia.Migration.Core.Mappers
{
    public static class SocialMapper
    {
       

        public static Omnia.Fx.Models.Social.Comment MapComment(int pageId, Guid? parentId, G1Comment comment, ItemQueryResult<IResolvedIdentity> Identities)
        {
            
            // Thoan modified 7.6 
            return new Omnia.Fx.Models.Social.Comment()
            {
                Content = comment.Content,
                CreatedBy = (AuthenticatableIdentity)UserMaper.GetIdentitybyEmail(Identities, comment.CreatedBy),
                CreatedAt = comment.CreatedAt,
                ModifiedAt = comment.ModifiedAt,

                TopicId = Omnia.WebContentManagement.Fx.Constants.TopicPrefixes.Page + pageId,
                ParentId = parentId
            };
        }
        public static Omnia.Fx.Models.Social.Like MapLike(int pageId, G1Like like, ItemQueryResult<IResolvedIdentity> Identities)
        { 
            // Thoan modified 7.6
            return new Omnia.Fx.Models.Social.Like()
            {
                CommentId = like.CommentId,
                CreatedBy = (AuthenticatableIdentity)UserMaper.GetIdentitybyEmail(Identities, like.CreatedBy),
                CreatedAt = like.CreatedAt,
                ModifiedBy = (AuthenticatableIdentity)UserMaper.GetIdentitybyEmail(Identities, like.ModifiedBy),
                ModifiedAt = like.ModifiedAt,
                TopicId = Omnia.WebContentManagement.Fx.Constants.TopicPrefixes.Page + pageId
            };
        }
        private static identity GetUserIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return new identity() { id = item.Id, type = item.Type };
                }
            }
            return null;
        }
        // Thoan modified 7.6
       
    }

}

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
using static Omnia.Migration.Core.Mappers.UserMaper;


namespace Omnia.Migration.Core.Mappers
{
    public static class UserMaper
    {
        public class ShortIdentity
        {
            public string id { get; set; }
           public int type { get; set; }
        }




        // Thoan modified 7.6
        public static Identity GetIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
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
        public static string GetSystemPropUserIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return item.Id + "[1]";
                }
            }
            return null;
        }
        public static ShortIdentity GetShortIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return new ShortIdentity() { id = item.Id.ToString(),type=1};
                }
            }
            return null;
        }
    }
    
}



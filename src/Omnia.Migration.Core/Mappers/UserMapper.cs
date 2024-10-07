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


namespace Omnia.Migration.Core.Mappers
{
    public static class UserMaper
    {
       
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
    }

}

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.UserProfiles;
using Microsoft.SharePoint.Client.Utilities;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Helpers
{
	public static class SPHelper
	{
		//Hieu rem
		/*public static async ValueTask<string> GetUserLoginNameByEmailAsync(ClientContext ctx, string email)
		{
			Web web = ctx.Web;
			PeopleManager peopleManager = new PeopleManager(ctx);
			ClientResult<PrincipalInfo> principal = Utility.ResolvePrincipal(ctx, web, email, PrincipalType.User, PrincipalSource.All, web.SiteUsers, true);
			await ctx.ExecuteQueryAsync();

			return principal.Value.LoginName;
		}*/

		public static int GetAppTypeId(string spTemplate)
		{
			switch (spTemplate.ToLower())
			{
				case "group#0":
					{
						return Constants.TeamWorkAppType.Office365Group;
					}
				case "sts#3":
					{
						return Constants.TeamWorkAppType.SharePointTeamSite;
					}
				case "sts#0":
					{
						return Constants.TeamWorkAppType.SharePointTeamSite;
					}
			}
			return Constants.TeamWorkAppType.SharePointTeamSite;
		}
		public static async ValueTask<User> EnsureUser(ClientContext clientContext, string emailAddress)
		{
			User user = null;
			try
			{
				var result = Utility.ResolvePrincipal(clientContext, clientContext.Web, emailAddress, PrincipalType.User, PrincipalSource.All, null, true);
				await clientContext.ExecuteQueryAsync();

				if (result.Value != null)
				{
					user = clientContext.Web.EnsureUser(result.Value.LoginName);
					clientContext.Load(user);
					await clientContext.ExecuteQueryAsync();
				}
			}
			catch (Exception ex)
			{
				return null;

			}

			return user;
		}

		//Hieu rem
		/*public static async ValueTask<List<string>> GetFailedUser(ClientContext clientContext, List<string> emailAddress)
		{
			var failedUser = new List<string>();

			foreach (var email in emailAddress)
			{
				var user = await EnsureUser(clientContext, email);
				if (user.IsNull())
				{
					failedUser.Add(email);
				}
			}

			return failedUser;
		}*/
		//Hieu added
		public static async ValueTask<List<Identity>> GetFailedUserIdentities(ClientContext clientContext, List<string> emailAddress, ItemQueryResult<IResolvedIdentity> Identities)
		{
			var failedUser = new List<Identity>();

			foreach (var email in emailAddress)
			{
				var user = await EnsureUser(clientContext, email);
				if (user.IsNull())
				{
					failedUser.Add(GetUserIdentitybyEmail(Identities, email));
				}
			}

			return failedUser;
        }
        private static Identity GetUserIdentitybyEmail(ItemQueryResult<IResolvedIdentity> Identities, string email)
        {
            foreach (ResolvedUserIdentity item in Identities.Items)
            {
                if (item.Username.Value.Text.ToLower() == email.ToLower())
                {
                    return new UserIdentity(item.Id);
                }
            }
            return null;
        }
        //<<
    }
}

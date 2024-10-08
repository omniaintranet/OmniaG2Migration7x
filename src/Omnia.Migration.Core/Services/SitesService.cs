using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.Migration.Models.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
	public class SitesService
	{
		private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
		private G1FeatureApiHttpClient FeatureApiHttpClient { get; }
		public SitesService(
			G1FeatureApiHttpClient featureApiHttpClient,
			IOptionsSnapshot<MigrationSettings> migrationSettings)
		{
			FeatureApiHttpClient = featureApiHttpClient;
			MigrationSettings = migrationSettings;
		}

		public async ValueTask<List<string>> GetSitesWithG1Feature(Guid featureId)
		{
			var getFeaturesResult = await FeatureApiHttpClient.GetFeatureInfoAsync(featureId, MigrationSettings.Value.WCMContextSettings.SharePointUrl);
			if (!getFeaturesResult.IsSuccess)
				throw new Exception("Error getting feature instance for {" + featureId + "} : " + getFeaturesResult.ErrorMessage);

			var modernSites = getFeaturesResult.Data.Instances
				.Where(x => x.Status == Foundation.Models.Features.FeatureInstanceStatus.Activated)
				.Select(x => x.Target)
				.ToList();

			return modernSites;
		}

		public async ValueTask<List<string>> GetG1SPFxSitesAsync()
		{
			return await GetSitesWithG1Feature(Constants.G1FeatureIds.SpfxInfrastructure);
		}

		public async ValueTask<List<string>> GetG1MasterPageSitesAsync()
		{
			return await GetSitesWithG1Feature(Constants.G1FeatureIds.CoreMasterPage);
		}

		public async ValueTask<ListItem> GetSiteDirectoryInfoFileAsync(ClientContext clientContext)
		{
			try
			{
				var omniaList = clientContext.Web.Lists.GetByTitle("Omnia");

				CamlQuery camlQuery = new CamlQuery();
				camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='FileLeafRef'/>" +
					"<Value Type='Text'>omnia-sitedirectory.txt</Value></Eq></Where></Query><RowLimit>100</RowLimit></View>";


				ListItemCollection listItems = omniaList.GetItems(camlQuery);

				clientContext.Load(listItems);
				await clientContext.ExecuteQueryAsync();

				var siteDirectoryInfo = listItems.FirstOrDefault();

				return siteDirectoryInfo;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public async Task RemoveG1SPFxHeaderAsync(ClientContext clientContext)
		{
			var toDelete = await GetOmniaG1CustomerActionAsync(clientContext);
			await RemoveCustomActionsAsync(clientContext, toDelete);
		}

		public async Task RemoveCustomActionsAsync(ClientContext clientContext, List<UserCustomAction> customActions)
		{
			for (int i = 0; i < customActions.Count(); i++)
			{
				customActions[i].DeleteObject();
			}
			await clientContext.ExecuteQueryAsync();
		}

		public async ValueTask<List<UserCustomAction>> GetOmniaG1CustomerActionAsync(ClientContext clientContext)
		{
			clientContext.Load(clientContext.Site.UserCustomActions, x => x.Include(y => y.Name));
			await clientContext.ExecuteQueryAsync();

			return clientContext.Site.UserCustomActions.Where(x => x.Name.Equals("OmniaApplicationCustomizer")).ToList();
		}

		public async ValueTask<List<UserCustomAction>> GetOmniaG2CustomActionAsync(ClientContext clientContext)
		{
			clientContext.Load(clientContext.Site.UserCustomActions, x => x.Include(y => y.Name));
			await clientContext.ExecuteQueryAsync();

			return clientContext.Site.UserCustomActions.Where(x => x.Name.Equals("OmniaG2ApplicationCustomizer")).ToList();
		}

		public dynamic GenerateCreateSiteProperties(SiteMigrationItem site)
		{
			dynamic createSiteProperties = new Newtonsoft.Json.Linq.JObject();
			createSiteProperties.spPath = UrlHelper.GetRelativeUrl(site.SiteUrl);
			createSiteProperties.spAlias = UrlHelper.GetRelativeUrl(site.SiteUrl).Split("/").Last(); // Temp fix for Omnia 4.0
			createSiteProperties.omniaPath = UrlHelper.GetRelativeUrl(site.SiteUrl).Split("/").Last();//Temp fix for Omnia 6.10
			createSiteProperties.omniaRoutePrefix = "_t";//Temp fix for Omnia 6.10
			createSiteProperties.lcid = site.LCID.ToString();
			createSiteProperties.timezoneid = site.TimeZoneId.ToString();
			createSiteProperties.isSiteAttached = true;
			createSiteProperties.appType = SPHelper.GetAppTypeId(site.SPTemplate); // TODO: is there other type? yes 6 types in total
			createSiteProperties.location = "/sites";
			createSiteProperties.owner = site.PermissionIdentities.Admin[0];
			//createSiteProperties.appAdministrators = JToken.FromObject(new string[] { site.PermissionIdentities.Admin[0] });
			return createSiteProperties;
		}

		public SiteTemplateMapping GetSiteTemplateMapping(Dictionary<string, SiteTemplateMapping> siteTemplateMappings, SiteMigrationItem site)
		{
			var g1TemplateId = site.G1SiteTemplateId?.ToLower();
			SiteTemplateMapping siteTemplateMapping = null;
			if (!string.IsNullOrEmpty(g1TemplateId) && siteTemplateMappings.ContainsKey(g1TemplateId))
			{
				siteTemplateMapping = siteTemplateMappings[g1TemplateId];
			}
			else if (site.GroupId != Guid.Empty && siteTemplateMappings.ContainsKey(Constants.Configurations.SiteTemplatesMapping.O365GroupDefaultMapping))
			{
				siteTemplateMapping = siteTemplateMappings[Constants.Configurations.SiteTemplatesMapping.O365GroupDefaultMapping];
			}
			else if (siteTemplateMappings.ContainsKey(Guid.Empty.ToString()))
			{

				siteTemplateMapping = siteTemplateMappings[Guid.Empty.ToString()];
			}

			return siteTemplateMapping;
		}

		public Dictionary<string, object> ExtractSitePropertiesFromG1(ListItem siteDirectoryInfo, WCMContextSettings wcmSettings)
		{
			var result = new Dictionary<string, object>();

			if (siteDirectoryInfo != null)
			{
				var siteProps = siteDirectoryInfo.FieldValues.Keys.Where(x => x.StartsWith("odp"));
				foreach (var prop in siteProps)
				{
					var propMapping = wcmSettings.SiteTemplateMappings.FirstOrDefault(mapping => mapping.Properties.ContainsKey(prop))?.Properties[prop];
					if (propMapping != null && siteDirectoryInfo.FieldValues[prop] != null)
					{
						var oldValue = JToken.FromObject(siteDirectoryInfo.FieldValues[prop]);

						switch (propMapping.PropertyType)
						{
							case Models.EnterpriseProperties.EnterprisePropertyType.User:
								if (oldValue is JArray)
								{
									var newValue = new List<String>();
									foreach (var item in oldValue)
									{
										if (item != null && item["Email"] != null)
											newValue.Add(item["Email"].ToString());
									}
									result.Add(prop, newValue);
								}
								else
								{
									result.Add(prop, oldValue != null && oldValue["Email"] != null ? new List<string> { oldValue["Email"].ToString() } : null);
								}
								break;
							case Models.EnterpriseProperties.EnterprisePropertyType.Taxonomy:
								result.Add(prop, oldValue != null ? new List<object> { oldValue } : null);
								break;
							default:
								result.Add(prop, oldValue);
								break;
						}
					}
				}
			}

			return result;
		}

		private List<string> GetEmailWithSelectedPersonProperty(SiteMigrationItem site, List<string> selectedUserProfileProperties)
		{
			var resultEmailList = new List<string>();

			var emailPropety = site.EnterpriseProperties
						.Where(s => selectedUserProfileProperties.Any(i => i == s.Key))
						.Select(m => m.Value).ToList();

			foreach (var email in emailPropety)
			{
				try
				{
					var emailList = JsonConvert.DeserializeObject<List<string>>(email.ToString());

					foreach (var e in emailList)
					{
						resultEmailList.Add(e.ToString());
					}
				}
				catch (Exception)
				{
					try
					{
						var oldValue = JToken.FromObject(email);
						oldValue = JToken.FromObject(oldValue.Values<string>("Email"));
						var users = oldValue.ToObject<List<string>>().Where(x => !string.IsNullOrEmpty(x)).ToList();
						foreach (var user in users)
						{
							var isEmail = CommonUtils.IsValidEmail(user.ToString());
							if (isEmail)
							{
								resultEmailList.Add(user.ToString());
							}
						}
					}
					catch (Exception e)
					{
						resultEmailList.Add(email.ToString());
					}
				}
				finally
				{
				}
			}
			return resultEmailList;
		}

		private List<string> GetEmailWithEnterpriseProperties(SiteMigrationItem site)
		{
			var resultEmailList = new List<string>();

			var emailPropety = site.EnterpriseProperties
					.Where(e => e.Value.ToString().Contains('@'))
					.Select(m => m.Value).ToList();

			foreach (var email in emailPropety)
			{
				try
				{
					var emailList = JsonConvert.DeserializeObject<List<string>>(email.ToString());

					foreach (var e in emailList)
					{
						var isEmail = CommonUtils.IsValidEmail(e.ToString());
						if (isEmail)
						{
							resultEmailList.Add(e.ToString());
						}
					}
				}
				catch(Exception)
				{
					try
                    {
						var oldValue = JToken.FromObject(email);
						oldValue = JToken.FromObject(oldValue.Values<string>("Email"));
						var users = oldValue.ToObject<List<string>>().Where(x => !string.IsNullOrEmpty(x)).ToList();
						foreach (var user in users)
						{
							var isEmail = CommonUtils.IsValidEmail(user.ToString());
							if (isEmail)
							{
								resultEmailList.Add(user.ToString());
							}
						}
					}
					catch (Exception e)
					{
						resultEmailList.Add(email.ToString());
					}									
				}
				finally
				{

				}
			}

			return resultEmailList;
		}

		private List<string> GetGroupMemberShipEmail(SiteMigrationItem site, List<string> selectedPersonProperties)
		{
			var resultEmailList = new List<string>();

			if (site.EnterpriseProperties.Count > 0)
			{
				if (selectedPersonProperties.Count > 0)
				{
					resultEmailList.AddRange(GetEmailWithSelectedPersonProperty(site, selectedPersonProperties));
				}
				else
				{
					resultEmailList.AddRange(GetEmailWithEnterpriseProperties(site));
				}
			}
			return resultEmailList;
		}

        //Hieu rem
        /*public List<string> GetAppAdministratorsEmail(SiteMigrationItem site, List<string> existedList)
		{
			var emailList = new List<string>();

			if (site.PermissionIdentities.Admin.Count > 0)
			{
				var filterList = site.PermissionIdentities.Admin.Where(s => !existedList.Any(e => e == s)).ToList();

				if (filterList.Count > 0)
				{
					emailList.AddRange(filterList);
				}

			}

			return emailList;
		}*/
        public List<string> GetAppAdministratorsEmail(SiteMigrationItem site, List<string> existedList)
        {
            var emailList = new List<string>();

            if (site.PermissionIdentities.Admin.Count > 0)
            {
                var filterList = site.PermissionIdentities.Admin.Where(s => !existedList.Any(e => e == s)).ToList();

                if (filterList.Count > 0)
                {
					//Hieu rem
                   // emailList.AddRange(filterList);
                }

            }

            return emailList;
        }
        //Hieu rem
        public async Task EnsureFailedUser(ClientContext clientContext, SiteMigrationItem site, List<string> selectedPersonProperties, ItemQueryResult<IResolvedIdentity> Identities)
		{
			try
			{
				var ensureEmailList = new List<string>() { };

				var groupMemberShipEmail = GetGroupMemberShipEmail(site, selectedPersonProperties);

				if (groupMemberShipEmail.Count > 0)
				{
					ensureEmailList.AddRange(groupMemberShipEmail);
				}

				var appAdministratorsEmail = GetAppAdministratorsEmail(site, ensureEmailList);

				if (appAdministratorsEmail.Count > 0)
				{
					ensureEmailList.AddRange(appAdministratorsEmail);
				}

				if (ensureEmailList.Count > 0)
				{
					//Hieu rem
                    //var failedUser = await SPHelper.GetFailedUser(clientContext, ensureEmailList);
                    //if (failedUser.Count > 0)
                    //{
                    //	ImportSitesReport.Instance.AddFailedUser(site, failedUser);
                    //}

                    var failedUser = await SPHelper.GetFailedUserIdentities(clientContext, ensureEmailList, Identities);
                    if (failedUser.Count > 0)
                    {
                        ImportSitesReport.Instance.AddFailedUser(site, failedUser);
                    }
                }
			}
			catch (Exception)
			{

			}
			finally
			{

			}
		}
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Migration.Models;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.WebContentManagement.Models.Navigation;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    // Clone helper to work around casting issue with OmniaJsonBase class
    public static class CloneHelper
    {
        public static NavigationNode<NavigationData> CloneToNavigationNode(IPageNavigationNode pageNode)
        {
            return Clone<NavigationNode<NavigationData>>(pageNode);
        }

        public static NavigationNode<NavigationData> CloneToNavigationNode(PageNavigationNode<PageNavigationData> pageNode)
        {
            return Clone<NavigationNode<NavigationData>>(pageNode);
        }

        public static PageNavigationMigrationItem CloneToPageMigration(NavigationMigrationItem navMigrationItem)
        { //navMigrationItem.AdditionalProperties["PageData"]
            //Hieu added
            CorrectLayouts(navMigrationItem);
            //<<
            return Clone<PageNavigationMigrationItem>(navMigrationItem);
        }


        private static void CorrectLayouts(NavigationMigrationItem navMigrationItem)
        {

            if (navMigrationItem.MigrationItemType == NavigationMigrationItemTypes.Page)
            {
                var jsonObj = navMigrationItem.AdditionalProperties["PageData"];
                jsonObj["Layout"] = JToken.FromObject(new Omnia.Fx.Models.Layouts.Layout());
                //navMigrationItem.AdditionalProperties["PageData"] = jsonObj;


                var jsonTranslations = navMigrationItem.AdditionalProperties["TranslationPages"];
                var variations = jsonTranslations.ToObject<List<NavigationMigrationItem>>();
                if (variations != null)
                {
                    foreach (var variation in variations)
                    {
                        var jsonObjvar = variation.AdditionalProperties["PageData"];
                        jsonObjvar["Layout"] = JToken.FromObject(new Omnia.Fx.Models.Layouts.Layout());
                    }
                    navMigrationItem.AdditionalProperties["TranslationPages"] = JToken.FromObject(variations);
                }
                foreach (var child in navMigrationItem.Children)
                {
                    CorrectLayouts(child);
                }
            }
        }
        public static LinkNavigationMigrationItem CloneToLinkMigration(NavigationMigrationItem navMigrationItem)
        {
            return Clone<LinkNavigationMigrationItem>(navMigrationItem);
        }

        public static T Clone<T>(object obj)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj));
        }
    }
}

using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core.Mappers
{
    public static class SiteMapper
    {
        public static void MapSiteData(SiteMigrationItem site, MigrationSettings settings)
        {
            MapEnterpriseProperties(site, settings.WCMContextSettings);
        }

        private static void MapEnterpriseProperties(SiteMigrationItem site, WCMContextSettings wcmSettings)
        {

        }
    }
}

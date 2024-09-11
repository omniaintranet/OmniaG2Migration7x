using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.MigrationItem
{
    public class LinkNavigationMigrationItem: NavigationMigrationItem
    {
        public string Title { get; set; }

        public string Url { get; set; }
    }
}

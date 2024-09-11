using Omnia.Fx.Models.JsonTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.MigrationItem
{
    public class NavigationMigrationItem: OmniaJsonBase
    {
        public int? Id { get; set; }

        public int? ParentId { get; set; }

        public List<NavigationMigrationItem> Children { get; set; }

        public bool ShowInMegaMenu { get; set; }

        public bool ShowInCurrentNavigation { get; set; }

        public NavigationMigrationItemTypes MigrationItemType { get; set; }

        public NavigationMigrationItem()
        {
            Children = new List<NavigationMigrationItem>();
        }
    }

    public enum NavigationMigrationItemTypes
    {
        Link = 0,
        Page = 1
    }
}

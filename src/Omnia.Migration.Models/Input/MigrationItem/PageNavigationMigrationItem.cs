using Omnia.Migration.Models.BlockData;
using Omnia.Migration.Models.Input.BlockData;
using Omnia.Migration.Models.Input.Social;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Input.MigrationItem
{    
    public class PageNavigationMigrationItem: NavigationMigrationItem
    {
        public PageData PageData { get; set; }

        public string MainContent { get; set; }        

        public string UrlSegment { get; set; }

        public Guid? GlueLayoutId { get; set; }

        public List<RelatedLink> RelatedLinks { get; set; }

        public List<G1Comment> Comments { get; set; }

        public List<G1Like> Likes { get; set; }

        public List<PageNavigationMigrationItem> TranslationPages { get; set; }

        public List<G1BlockSetting> BlockSettings { get; set; }

        public string PageLanguage { get; set; }

        public string CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public string ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }

        public Guid PhysicalPageUniqueId { get; set; }

        public PageNavigationMigrationItem()
        {
            BlockSettings = new List<G1BlockSetting>();
            TranslationPages = new List<PageNavigationMigrationItem>();
            Comments = new List<G1Comment>();
            Likes = new List<G1Like>();
            RelatedLinks = new List<RelatedLink>();
        }
    }

    public class PageNavigationMigrationItem1 : NavigationMigrationItem
    {
        public PageData PageData { get; set; }  
    }
}

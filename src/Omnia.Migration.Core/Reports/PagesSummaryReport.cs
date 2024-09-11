using Omnia.Migration.Models.Configuration;
using Microsoft.Azure.Amqp.Framing;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.BlockData;
using Omnia.Migration.Models.Mappings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Core.Reports
{
    public class PagesSummaryReport : BaseMigrationReport
    {
        #region Constructor

        static PagesSummaryReport()
        { }

        private PagesSummaryReport()
        {

        }

        public static PagesSummaryReport Instance { get; } = new PagesSummaryReport();

        #endregion

        public int NumberOfPagesWithDocumentRollup => PagesWithDocumentRollup.Count;

        public int NumberOfPagesWithControlledDocumentView => PagesWithControlledDocumentView.Count;

        public int NumberOfPagesWithPeopleRollup => PagesWithPeopleRollup.Count;

        public int NumberOfPagesWithBanner => PagesWithBanner.Count;

        public int NumberOfPageWithOtherBlocks => PagesWithOtherBlocks.Count;

        public HashSet<string> EnterpriseProperties { get; set; }

        public HashSet<G1SearchProperty> SearchProperties { get; set; }

        public HashSet<SearchPropertyMapping> SearchPropertyMappings { get; set; }

        public HashSet<string> PagesWithDocumentRollup { get; set; }

        public HashSet<string> PagesWithControlledDocumentView { get; set; }

        public HashSet<string> PagesWithPeopleRollup { get; set; }

        public HashSet<string> PagesWithBanner { get; set; }

        public HashSet<string> PagesWithOtherBlocks { get; set; }   
        
        public Dictionary<Guid, List<string>> PagesWithOtherBlocksDetails { get; set; }

        public HashSet<string> PagesUnderLink { get; set; }

        public Dictionary<Guid, HashSet<string>> CustomPageLayouts { get; set; }
        public Dictionary<Guid, HashSet<string>> CustomPageLayoutZones { get; set; }

        public HashSet<string> AllPages { get; set; }

        public override string ReportName => "PagesSummaryReport";

        public override void Init(MigrationSettings settings)
        {
            base.Init(settings);

            EnterpriseProperties = new HashSet<string>();
            SearchProperties = new HashSet<G1SearchProperty>();
            SearchPropertyMappings = new HashSet<SearchPropertyMapping>();
            PagesWithDocumentRollup = new HashSet<string>();
            PagesWithControlledDocumentView = new HashSet<string>();
            PagesWithPeopleRollup = new HashSet<string>();
            PagesWithBanner = new HashSet<string>();
            PagesWithOtherBlocks = new HashSet<string>();
            PagesWithOtherBlocksDetails = new Dictionary<Guid, List<string>>();
            PagesUnderLink = new HashSet<string>();
            CustomPageLayouts = new Dictionary<Guid, HashSet<string>>();
            CustomPageLayoutZones = new Dictionary<Guid, HashSet<string>>();
            AllPages = new HashSet<string>();
        }

        public void AddEnterpriseProperty(string property)
        {
            EnterpriseProperties.Add(property);
        }

        public void AddSearchProperty(G1SearchProperty property, string propertyDisplayName)
        {
            SearchProperties.Add(property);

            if (!SearchPropertyMappings.Any(x => x.G1PropertyId == property.id))
            {
                var g1PropertyGuid = property.id ?? Guid.Empty;
                SearchPropertyMappings.Add(new SearchPropertyMapping
                {
                    G1PropertyId = g1PropertyGuid,
                    G1PropertyName = propertyDisplayName,
                    G2PropertyName = string.Empty,
                    ManagedPropertyName = property.managedProperty
                });
            }
        }

        public void AddPageWithOtherBlocks(string pageUrl, Guid blockId)
        {
            PagesWithOtherBlocks.Add(pageUrl);

            if (!PagesWithOtherBlocksDetails.ContainsKey(blockId))
            {
                PagesWithOtherBlocksDetails.Add(blockId, new List<string>());
            }

            PagesWithOtherBlocksDetails[blockId].Add(pageUrl);
        }

        public void AddCustomPageLayout(Guid pageLayoutId, List<G1BlockSetting> blockSettings, string pageUrl)
        {
            if (!CustomPageLayouts.ContainsKey(pageLayoutId))
            {
                CustomPageLayouts.Add(pageLayoutId, new HashSet<string>());
                CustomPageLayoutZones.Add(pageLayoutId, new HashSet<string>());
            }

            CustomPageLayouts[pageLayoutId].Add(pageUrl);

            foreach (var block in blockSettings)
            {
                CustomPageLayoutZones[pageLayoutId].Add(block.ZoneId);
            }
        }
    }
}

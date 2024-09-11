using Omnia.Fx.Models.EnterpriseProperties;
using Omnia.WebContentManagement.Models.Navigation;
using Omnia.WebContentManagement.Models.Pages;
using Omnia.WebContentManagement.Models.Variations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Models.Shared
{
    public class WcmBaseData
    {
        public int? PageCollectionId { get; set; }

        public Guid SecurityResourceId { get; set; }

        public INavigationNode PageCollectionNode { get; set; }

        public IList<INavigationNode> ExistingNodes { get; set; }

        public Dictionary<int, PublishedVersionPageData<PageData>> PageTypes { get; set; }

        public IList<Variation> Variations { get; set; }

        public IList<EnterprisePropertyDefinition> EnterpriseProperies { get; set; }

        public Variation DefaultVariation { get; set; }
    }
}

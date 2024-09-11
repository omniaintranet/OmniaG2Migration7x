using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.WebContentManagement.Models.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    public static class NavigationNodeHelper
    {
        public static PageNavigationNode<PageNavigationData> FindMatchingNavigationNode(
            PageNavigationMigrationItem currentPage, 
            IList<INavigationNode> allNodes, 
            INavigationNode parentNode)
        {
            var currentUrlSegment = currentPage.UrlSegment.ToLower();

            var matchingNode = allNodes.FirstOrDefault(node =>
                node.Position.ParentNodeId == parentNode.Id &&
                node.NavigationNodeType == NavigationNodeType.Page &&
                ((PageNavigationNode<PageNavigationData>)node).NavigationData.UrlSegment.ToLower() == currentUrlSegment);

            return matchingNode as PageNavigationNode<PageNavigationData>;
        }

        public static INavigationNode FindMatchingNavigationNode(LinkNavigationMigrationItem currentLink,
            IList<INavigationNode> allNodes,
            INavigationNode parentNode)
        {
            var linkUrl = currentLink.Url.ToLower();

            var matchingNode = allNodes.FirstOrDefault(node =>
                node.Position.ParentNodeId == parentNode.Id &&
                node.NavigationNodeType == NavigationNodeType.Generic &&
                node.NavigationData.AdditionalProperties["url"].ToString().ToLower() == linkUrl);

            return matchingNode;
        }        
    }
}

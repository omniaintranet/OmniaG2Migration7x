using Omnia.Fx.Models.Layouts;
using Omnia.Migration.Core.Extensions;
using Omnia.WebContentManagement.Models.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Core.Helpers
{
    /// <summary>
    /// Class to handle the layout operations and apply Page Types
    /// </summary>
    public class LayoutManager
    {
        public static Guid? ExtractBlockIdForPageProperty(Layout layout, string propertyName)
        {
            foreach (var block in layout.BlockSettings)
            {                
                var blockSettings = block.Value;
                if (blockSettings == null) continue;
                if (blockSettings.AdditionalProperties.ContainsKey("pageProperty") &&
                    blockSettings.AdditionalProperties["pageProperty"].ToString() == propertyName)
                    return block.Key;
            }

            return null;
        }

        public static Guid? ExtractContainerIdForLayoutItem(List<LayoutItem> layoutItems, Guid? targetItemId)
        {
            if (targetItemId.IsNullOrEmpty())
                return null;

            foreach (var layoutItem in layoutItems)
            {
                if (layoutItem.Items == null)
                    continue;

                if (layoutItem.Items.Any(x => x.Id == targetItemId.Value))
                    return layoutItem.Id;

                var result = ExtractContainerIdForLayoutItem(layoutItem.Items, targetItemId);
                if (result != null && result != Guid.Empty)
                    return result;
            }

            return null;
        }

        public static LayoutItem FindLayoutItemRecursive(LayoutItem root, Guid itemId)
        {
            if (root.Id == itemId)
                return root;
            if (root.Items == null)
                return null;

            foreach (var child in root.Items)
            {
                var result = FindLayoutItemRecursive(child, itemId);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Extract all the layoutitems for the curren layout and removes layout items from parent
        /// the other layout items
        /// </summary>
        /// <param name="layout">The layout to be extracted</param>
        public static LayoutDefinition RemoveParentLayoutItems(LayoutDefinition layout)
        {
            List<LayoutItem> result = new List<LayoutItem>();
            ExtractLayoutItemsForLayout(layout.Id, layout.Id, layout.Items, result);
            layout.Items = result;
            return layout;
        }

        /// <summary>
        /// Injects a pagetype to the extracted alyout
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="pageTypeLayout"></param>
        /// <returns></returns>
        public static LayoutDefinition InjectParentLayout(LayoutDefinition layout, LayoutDefinition parentLayout)
        {
            List<LayoutItem> resultLayout = (parentLayout.Items != null) ? new List<LayoutItem>(parentLayout.Items) : new List<LayoutItem>();
            layout.Items.ForEach((layoutItem) =>
            {

                if (!InsertItemIntoLayout(layoutItem, resultLayout))
                {
                    /*Add all unmapped layout items to the end of the layout items*/
                    resultLayout.Add(layoutItem);
                };

            });
            layout.Items = resultLayout;
            return layout;
        }

        /// <summary>
        /// Inserts the item into the layout item structure
        /// </summary>
        /// <param name="item">Item to insert</param>
        /// <param name="items">Items to insert into</param>
        /// <returns>True if item container is found and the item is inserted</returns>
        private static bool InsertItemIntoLayout(LayoutItem itemToInsert, List<LayoutItem> items)
        {
            if (items == null)
            {
                return false;
            }
            for (int i = 0; i < items.Count; i++)
            {
                LayoutItem currentLayoutItem = items[i];
                if (currentLayoutItem.Id == itemToInsert.ContainerId)
                {
                    currentLayoutItem.Items = InsertSibling(itemToInsert, currentLayoutItem.Items);
                    return true;
                }
                else
                {
                    if (InsertItemIntoLayout(itemToInsert, currentLayoutItem.Items))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Inserts a sibling into the right index of the item array
        /// </summary>
        /// <param name="itemToInsert"></param>
        /// <param name="items"></param>
        private static List<LayoutItem> InsertSibling(LayoutItem itemToInsert, List<LayoutItem> items)
        {
            List<LayoutItem> result = items;
            if (result == null)
            {
                result = new List<LayoutItem>();
                result.Add(itemToInsert);
                return result;
            }
            if (itemToInsert.PrevSiblingId == Guid.Empty)
            {
                result.Insert(0, itemToInsert);
                return result;
            }
            for (int i = 0; i < items.Count; i++)
            {
                LayoutItem currentLayoutItem = items[i];
                if (currentLayoutItem.Id == itemToInsert.PrevSiblingId)
                {
                    result.Insert(i + 1, itemToInsert);
                    return result;
                }
            }
            /*If not match found add to the item array*/
            result.Add(itemToInsert);
            return result;
        }

        /// <summary>
        /// Flattens the layoutstructure to include blocks only from the current layout
        /// </summary>
        /// <param name="layoutitems"></param>
        /// <param name="result"></param>
        private static void ExtractLayoutItemsForLayout(Guid layoutId, Guid containerId, List<LayoutItem> layoutitems, List<LayoutItem> result)
        {
            if (layoutitems == null)
            {
                return;
            }
            for (int i = 0; i < layoutitems.Count; i++)
            {
                LayoutItem item = layoutitems[i];
                if (item.OwnerLayoutId == layoutId)
                {
                    item.ContainerId = containerId;
                    item.PrevSiblingId = (i == 0) ? Guid.Empty : layoutitems[i - 1].Id;
                    result.Add(item);
                }
                else
                {
                    ExtractLayoutItemsForLayout(layoutId, item.Id, item.Items, result);
                }
            }
        }
    }
}

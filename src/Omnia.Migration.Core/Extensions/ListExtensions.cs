using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnia.Migration.Core.Extensions
{
    public static class ListExtensions
    {
        public static int GetTotalCount(this List<NavigationMigrationItem> navigationNodes)
        {
            var count = navigationNodes.Count;
            navigationNodes.ForEach(node => count += node.Children.GetTotalCount());

            return count;
        }

        public static List<List<T>> Split<T>(this List<T> list, int numberOfSubLists)
        {
            var subListSize = list.Count / numberOfSubLists + 1;
            var subLists = new List<List<T>>();

            for (int i = 0; i < numberOfSubLists; i++)
            {
                var subList = list.Skip(i * subListSize).Take(subListSize).ToList();
                if (subList.Count == 0)
                    break;
                else
                    subLists.Add(subList);
            }

            return subLists;
        }
    }
}

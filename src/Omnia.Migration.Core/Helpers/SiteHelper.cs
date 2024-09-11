using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Helpers
{
	public static class SiteHelper
	{
		private static List<string> GetEnterprisePropertiesFromInput(List<SiteMigrationItem> input)
		{
			var enterpriseProperties = new List<string>();

			foreach (var site in input)
			{
				foreach (var property in site.EnterpriseProperties)
				{
					if (!enterpriseProperties.Contains(property.Key))
					{
						enterpriseProperties.Add(property.Key);
					}
				}
			}
			return enterpriseProperties;
		}

		public static List<string> SelectPersonProperties(List<SiteMigrationItem> input)
		{
			var selectedPersonProperties = new List<string>(); 
			var enterpriseProperties = GetEnterprisePropertiesFromInput(input);

			Console.WriteLine("Select user properties:");

			for (int i = 0; i < enterpriseProperties.Count; i++)
			{
				Console.WriteLine(string.Format("  ({0}) {1}", i, enterpriseProperties[i]));
			}
			Console.WriteLine("  (n) Done!");

			string key;
			do
			{
				key = Console.ReadLine();
				int inputNumber;
				bool isNumber = false;
				isNumber = int.TryParse(key, out inputNumber);

				if (isNumber)
				{
					int selectedIndex = inputNumber;
					if (!selectedPersonProperties.Contains(enterpriseProperties[selectedIndex]))
					{
						selectedPersonProperties.Add(enterpriseProperties[selectedIndex]);
					}
				}
				else
				{
					if (key == "n")
					{
						break;
					}
				}

				Console.WriteLine(string.Format("  Selected properties: "));

				for (int i = 0; i < selectedPersonProperties.Count; i++)
				{
					Console.Write(string.Format("  {0}", selectedPersonProperties[i]));
				}

				Console.WriteLine();
			}
			while (selectedPersonProperties.Count != enterpriseProperties.Count);

			return selectedPersonProperties;
		}
	}
}

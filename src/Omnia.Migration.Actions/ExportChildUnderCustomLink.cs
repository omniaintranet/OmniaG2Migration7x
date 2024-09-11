using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.Core.Extensions;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;

namespace Omnia.Migration.Actions
{
    public class ExportChildUnderCustomLink : BaseMigrationAction
    {
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private ILogger<GeneratePagesSummaryAction> Logger { get; }
        private List<string> PageList { get; set; }
        public ExportChildUnderCustomLink(
            IOptionsSnapshot<MigrationSettings> migrationSettings,
            ILogger<GeneratePagesSummaryAction> logger)
        {
            MigrationSettings = migrationSettings;
            Logger = logger;

        }

        public override async Task StartAsync(IProgressManager progressManager)
        {
            List<NavigationMigrationItem> input = ReadInput();
            progressManager.Start(input.GetTotalCount());
            PageList = new List<string>();

            foreach (var item in input)
            {
                if (item.MigrationItemType == NavigationMigrationItemTypes.Link)
                { 
                    CheckPage(item, string.Empty, progressManager); 
                }
            }

            File.WriteAllText(Path.Combine(MigrationSettings.Value.OutputPath, "ChilofCustomLink.json"), JsonConvert.SerializeObject(PageList, Formatting.Indented));
        }

        private void CheckPage(NavigationMigrationItem navigationMigrationItem, string parentPageUrl, IProgressManager progressManager)
        {
            string pageUrl = string.Empty;
            
            var linkNode = CloneHelper.CloneToLinkMigration(navigationMigrationItem);
            pageUrl = $"{parentPageUrl}/{linkNode.Url.ToLower().Replace(" ", "-")}";
            progressManager.ReportProgress(1);
            
            foreach (var item in navigationMigrationItem.Children)
            {
                CheckPage(item, pageUrl, progressManager);
            }
            PageList.Add(pageUrl);
        }

        private List<NavigationMigrationItem> ReadInput()
        {
            var inputPath = Path.Combine(MigrationSettings.Value.InputPath, MigrationSettings.Value.ImportPagesSettings.InputFile);
            var inputStr = File.ReadAllText(inputPath);
            if (!inputStr.StartsWith("[") && !inputStr.EndsWith("]"))
            {
                inputStr = "[" + inputStr + "]";
            }
            var input = JsonConvert.DeserializeObject<List<NavigationMigrationItem>>(inputStr);
            return input;
        }

    }
}

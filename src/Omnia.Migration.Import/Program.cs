using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Omnia.Migration.App.Helpers;
using Omnia.Migration.Models.Configuration;
using System;
using System.IO;
using System.Linq;
using Omnia.Migration.Actions;
using Omnia.Migration.Core.Helpers;
using System.Text;

namespace Omnia.Migration.App
{
    class Program
    {        
        static ILogger<Program> Logger { get; set; }

        static void Main(string[] args)
        {
            Console.Write("G2 Import - Version 3.0 - 22 June 2022");
            LoadSettings();

            var selectedAction = SelectAction();
            while (selectedAction != MigrationActions.Exit)
            {
                ExecuteAction(selectedAction);
                selectedAction = SelectAction();
            }

            Console.WriteLine("Finished - Press any key to quit...");
        }

        static void Init()
        {
            ServiceFactory.Setup();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };

            Logger = ServiceFactory.GetRequiredService<ILogger<Program>>();
        }

        static MigrationActions SelectAction()
        {            
            var selectedOption = ConsoleHelper.PromptForOptions("Select actions:", typeof(MigrationActions));
            return (MigrationActions)selectedOption;
        }

        static void ExecuteAction(MigrationActions action)
        {
            try
            {
                Console.WriteLine();

                switch (action)
                {
                    case MigrationActions.LoadSettings:
                        LoadSettings();
                        break;
                    case MigrationActions.ImportPages:
                        Console.WriteLine("Starting import pages task");
                        ExecuteMigrationAction<ImportPagesAction>("Importing pages", "Importing pages");
                        break;
                    case MigrationActions.ImportSharedLinks:
                        Console.WriteLine("Starting import shared links task");
                        ExecuteMigrationAction<ImportSharedLinksAction>("Importing shared links", "Importing shared links");
                        break;
                    case MigrationActions.ImportMyLinks:
                        Console.WriteLine("Starting import links task");
                        ExecuteMigrationAction<ImportMyLinksAction>("Importing my links", "Importing my links");
                        break;
                    case MigrationActions.ImportAnnouncements:
                        Console.WriteLine("Not implemented...");
                        break;
                    case MigrationActions.ImportTeamSites:
                        Console.WriteLine("Starting import sites task");
                        //ExecuteMigrationAction<ImportSitesAction>("Importing sites", "Importing sites");
                        break;
                    case MigrationActions.ExportTeamSites:
                        Console.WriteLine("Starting export sites task");
                        //ExecuteMigrationAction<ExportSitesAction>("Exporting sites", "Exporting sites");
                        break;
                    case MigrationActions.GeneratePagesSummaryReport:
                        Console.WriteLine("Starting generate pages summary task");
                        ExecuteMigrationAction<GeneratePagesSummaryAction>("Generating page summary", "Generating page summary");
                        break;
                    case MigrationActions.QueryPages:
                        Console.WriteLine("Querying pages");
                        ExecuteMigrationAction<QueryPageAction>("Querying pages", "Querying pages");
                        break;
                    case MigrationActions.QueryAppWithFeatureFailure:
                        Console.WriteLine("Query apps with feature failure");
                        ExecuteMigrationAction<QueryAppWithFeatureFailureAction>("Query apps with feature failure", "Query apps with feature failure");
                        break;
                    case MigrationActions.ExportChildUnderCustomLink:
                        Console.WriteLine("Export Child under Custom Links to csv file");
                        ExecuteMigrationAction<ExportChildUnderCustomLink>("Export Child under Custom links", "Export Child under Custom links");
                        break;
                    case MigrationActions.AppInstanceFeatureAction:
                        Console.WriteLine("Feature acction for sites");
                        ExecuteMigrationAction<AppInstanceFeatureAction>("Feature acction for sites", "Feature acction for sites");
                        break;
                    case MigrationActions.AppAdminPermissionAction:
                        Console.WriteLine("App Admin permission acction for sites");
                        ExecuteMigrationAction<SiteAppPermissionUpdate>("App Admin permissions", "App Admin permissions");
                        break;
                }

                if (Omnia.Migration.Core.Helpers.Logger.Logs.Count > 0)
                {
                    foreach (var log in Omnia.Migration.Core.Helpers.Logger.Logs)
                    {
                        Console.WriteLine(log);
                        Console.WriteLine();
                    }
                    Omnia.Migration.Core.Helpers.Logger.Logs = new System.Collections.Generic.List<string>();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        static void ExecuteMigrationAction<T>(string initialMsg, string progressMsg) where T: BaseMigrationAction
        {
            T migrationAction = ServiceFactory.GetRequiredService<T>();
            using (IProgressManager progressManager = new ProgressManager(initialMsg, progressMsg))
            {
                migrationAction.StartAsync(progressManager).Wait();
            }
        }

        static void LoadSettings()
        {
            try
            {
                string[] appSettingsFiles = Directory.GetFiles(".", "appsettings.*.json", SearchOption.AllDirectories).ToArray();
                string[] fileNames = appSettingsFiles.Select(p => p.Split("\\").Last()).ToArray();

                var selectedOption = ConsoleHelper.PromptForOptions("Select app settings:", fileNames);
                var settings = File.ReadAllText(appSettingsFiles[selectedOption], Encoding.UTF8);
                File.WriteAllText("appsettings.json", settings);

                Init();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when loading app settings: {ex.Message}");
            }
        }
    }
}


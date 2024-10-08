using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Omnia.Fx.Models.AppSettings;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Omnia.Migration.Actions;

namespace Omnia.Migration.App
{
    public static class ServiceFactory
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void Setup()
        {
            IConfiguration config = new ConfigurationBuilder()
             .AddJsonFile("appsettings.json", false, true)
             .Build();

            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Configure<MigrationSettings>(config.GetSection("MigrationSettings"));
            serviceCollection.Configure<OmniaServicesDnsSettings>(config.GetSection("OmniaServicesDnsSettings"));

            serviceCollection
                .AddHttpClient("omnia").ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    UseCookies = false
                    //cheat for onprem
                    //Credentials = new CredentialCache() {
                    //    {
                    //    new Uri ("https://intranet.hannessnellman.com/"),
                    //    "NTLM",
                    //    new NetworkCredential ("username","password")
                    //    }
                    // }
                });

            serviceCollection
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddFilter("System.Net.Http", LogLevel.None);
                    builder.AddConsole();
                    builder.AddDebug();
                })
                // HTTP Services
                .AddTransient<PageApiHttpClient>()
                .AddTransient<NavigationApiHttpClient>()
                .AddTransient<AppApiHttpClient>()
                .AddTransient<SocialApiHttpClient>()
                .AddTransient<SharedLinkApiHttpClient>()
                .AddTransient<VariationApiHttpClient>()
                .AddTransient<IdentityApiHttpClient>()
                .AddTransient<EnterprisePropertiesApiHttpClient>()
                .AddTransient<WcmImageApiHttpClient>()
                .AddTransient<AppTemplatesApiHttpClient>()
                .AddTransient<G1FeatureApiHttpClient>()
                .AddTransient<G1SearchPropertiesHttpClient>()
                .AddTransient<G1ODMSearchPropertiesHttpClient>()
                .AddTransient<G1SiteTemplatesHttpClient>()
                .AddTransient<CustomHttpImageClient>()
                .AddTransient<SharePointImageHttpClient>()
                .AddTransient<MyLinkApiHttpClient>()
                // Services
                .AddTransient<SocialService>()
                 .AddTransient<UserService>()// Thoan Add
                .AddTransient<ImagesService>()
                .AddTransient<SPTokenService>()
                .AddTransient<SitesService>()
                .AddTransient<PagesService>()
                .AddTransient<WcmService>()
                .AddTransient<LinksService>()
                // Migration Actions
                .AddTransient<ImportPagesAction>()
                .AddTransient<ImportSharedLinksAction>()
                .AddTransient<GeneratePagesSummaryAction>()
                //Hieu rem
                .AddTransient<ImportSitesAction>()      
                .AddTransient<ExportSitesAction>()
                .AddTransient<QueryPageAction>()
                .AddTransient<ImportMyLinksAction>()
                .AddTransient<QueryAppWithFeatureFailureAction>()
                .AddTransient<ExportChildUnderCustomLink>()
                .AddTransient<AppInstanceFeatureAction>()
                .AddTransient<AppInstanceFeatureAction>()
                .AddTransient<FeatureApiHttpClient>();



            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public static T GetRequiredService<T>()
        {
            if (ServiceProvider == null)
                throw new Exception("ServiceFactory is not setup");

            return ServiceProvider.GetRequiredService<T>();
        }
    }
}


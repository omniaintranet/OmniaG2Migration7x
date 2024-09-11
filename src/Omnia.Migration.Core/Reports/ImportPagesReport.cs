using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Input.MigrationItem;

namespace Omnia.Migration.Core.Reports
{
    public class ImportPagesReport: BaseMigrationReport
    {
        #region Constructor

        static ImportPagesReport()
        { }

        private ImportPagesReport()
        {

        }

        public static ImportPagesReport Instance { get; } = new ImportPagesReport();

        #endregion

        #region Properties

        public int NumberOfSucceedItems => SucceedItems.Count;

        public int NumberOfFailedItems => FailedItems.Count;        

        public ConcurrentBag<ImportedPageBase> SucceedItems { get; set; }

        public ConcurrentBag<ImportedPageBase> FailedItems { get; set; }
        public ConcurrentBag<ImportedPageBase> FailedImageItems { get; set; }
        public HashSet<string> FailedItemsURL { get; set; }

        public ConcurrentBag<ImportedPageBase> NewItems { get; set; }

        public override string ReportName => "ImportPages";

        #endregion

        #region Methods

        public override void Init(MigrationSettings settings)
        {
            base.Init(settings);
            
            SucceedItems = new ConcurrentBag<ImportedPageBase>();
            FailedItems = new ConcurrentBag<ImportedPageBase>();
            NewItems = new ConcurrentBag<ImportedPageBase>();
            FailedItemsURL = new HashSet<string>();
            FailedImageItems = new ConcurrentBag<ImportedPageBase>();
        }

        public void AddSucceedItem(PageNavigationMigrationItem migrationItem, int navigationNodeId, int pageId, string pagePath, Guid PhysicalPageUniqueId)
        {
            SucceedItems.Add(new ImportedPage(migrationItem, navigationNodeId, pageId, pagePath, PhysicalPageUniqueId));            
        }

        public void AddSucceedItem(LinkNavigationMigrationItem migrationItem, int navigationNodeId)
        {
            SucceedItems.Add(new ImportedLink(migrationItem, navigationNodeId));
        }

        public void AddNewItem(PageNavigationMigrationItem migrationItem, int navigationNodeId, int pageId, string pagePath, Guid PhysicalPageUniqueId)
        {
            NewItems.Add(new ImportedPage(migrationItem, navigationNodeId, pageId, pagePath, PhysicalPageUniqueId));
        }

        public void AddNewItem(LinkNavigationMigrationItem migrationItem, int navigationNodeId)
        {
            NewItems.Add(new ImportedLink(migrationItem, navigationNodeId));
        }

        public void AddFailedItem(PageNavigationMigrationItem migrationItem, int navigationNodeId, int pageId, string pagePath, Exception exception)
        {
            exception = getInnerExceptionMessage(exception);
            if(navigationNodeId == 99999999) //failed due to invalid image in the page
            {
                FailedImageItems.Add(new ImportedPage(migrationItem, navigationNodeId, pageId, pagePath, exception: exception));
            }
            else
            {
                FailedItems.Add(new ImportedPage(migrationItem, navigationNodeId, pageId, pagePath, exception: exception));
                FailedItemsURL.Add(pagePath);
            }            
        }

        public void AddFailedItem(LinkNavigationMigrationItem migrationItem, int navigationNodeId, Exception exception)
        {
            exception = getInnerExceptionMessage(exception);

            FailedItems.Add(new ImportedLink(migrationItem, navigationNodeId, exception));
        }

        private Exception getInnerExceptionMessage(Exception source)
		{
			Exception tartget = source;

			if (source.InnerException.IsNotNull())
			{
				string innerExceptionMessage = source.ToString();

				Exception fullMessageException = new Exception(innerExceptionMessage, source);
				tartget = fullMessageException;

			}

			return tartget;
        }

        #endregion

        #region Internel classes

        public class ImportedPageBase
        {
            public int NodeId { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string Exception { get; set; }

            //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            //public string Json { get; set; }

            public ImportedPageBase(NavigationMigrationItem migrationItem, int navigationNodeId, Exception exception = null)
            {
                NodeId = navigationNodeId;
                //Json = JsonConvert.SerializeObject(migrationItem);
                Exception = exception != null ? exception.Message + exception.StackTrace : null;
            }
        }

        public class ImportedPage : ImportedPageBase
        {
            public int PageId { get; set; }
            public string UrlSegment { get; set; }
            public string Path { get; set; }

            public Guid? G1PhysicalPageUniqueId { get; set; }

            public ImportedPage(PageNavigationMigrationItem migrationItem, int navigationNodeId, int pageId, string pagePath, Guid? PhysicalPageUniqueId = null, Exception exception = null)
                : base(migrationItem, navigationNodeId, exception)
            {
                UrlSegment = migrationItem.UrlSegment;
                PageId = pageId;
                Path = pagePath;
                G1PhysicalPageUniqueId = PhysicalPageUniqueId;
            }
        }

        public class ImportedLink : ImportedPageBase
        {
            public string Url { get; set; }

            public ImportedLink(LinkNavigationMigrationItem migrationItem, int navigationNodeId, Exception exception = null)
                : base(migrationItem, navigationNodeId, exception)
            {
                Url = migrationItem.Url;
            }
        }

        #endregion
    }
}

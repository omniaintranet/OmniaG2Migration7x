using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;

namespace Omnia.Migration.Core.Reports
{
    public class ImportPublishingChannelsReport : BaseMigrationReport
    {
        #region Constructor

        static ImportPublishingChannelsReport()
        { }

        private ImportPublishingChannelsReport()
        {

        }

        public static ImportPublishingChannelsReport Instance { get; } = new ImportPublishingChannelsReport();

        #endregion

        #region Properties

        public override string ReportName => "ImportPublishingChannels";

        #endregion

        #region Methods

        public override void Init(MigrationSettings settings)
        {
            base.Init(settings);       
        }

        #endregion
    }
}

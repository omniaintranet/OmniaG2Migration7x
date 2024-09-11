using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.App
{
    public enum MigrationActions
    {
        Exit = 0,
        LoadSettings = 1,
        ImportPages = 2,
        ImportSharedLinks = 3,
        ImportMyLinks = 4,
        ImportAnnouncements = 5,
        GeneratePagesSummaryReport = 6,
        ExportTeamSites = 7,
        ImportTeamSites = 8,
        QueryPages = 9,
        QueryAppWithFeatureFailure = 10,
        ExportChildUnderCustomLink = 11,
        AppInstanceFeatureAction = 12,
        AppAdminPermissionAction = 13
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Omnia.Migration.Core
{
    public class Constants
    {
        public class G1ControlIDs
        {
            public static readonly Guid Banner = new Guid("6F4F4334-1052-4AB4-A823-A3072951DB85");
            public const string BannerIdString = "6F4F4334-1052-4AB4-A823-A3072951DB85";

            public static readonly Guid DocumentRollup = new Guid("97AE49DF-2B1B-45C7-B3BE-873721313120");
            public const string DocumentRollupIdString = "97AE49DF-2B1B-45C7-B3BE-873721313120";

            public static readonly Guid PeopleRollup = new Guid("E03EEBBA-CA3D-46FF-ABFE-E75FFFAB5177");
            public const string PeopleRollupIdString = "E03EEBBA-CA3D-46FF-ABFE-E75FFFAB5177";

            public static readonly Guid NewsViewer = new Guid("FB58CF64-F53E-4A45-A489-DF747555834B");
            public const string NewsViewerIdString = "FB58CF64-F53E-4A45-A489-DF747555834B";

            // ODM
            public static readonly Guid ControlledDocumentView = new Guid("C07E8885-6B79-4A0A-A4DC-5522A78BB0F7");
            public const string ControlledDocumentViewIdString = "C07E8885-6B79-4A0A-A4DC-5522A78BB0F7";

            public static readonly Guid ScriptHtml = new Guid("82850DC5-971D-44DB-B058-BF8FD5D84FFD");
            public const string ScriptHtmlIdString = "82850DC5-971D-44DB-B058-BF8FD5D84FFD";

            public static readonly Guid Accordion = new Guid("1D0A2C83-87BC-4803-8B51-0574F9EA6CD4");
            public const string AccordionIdString = "1D0A2C83-87BC-4803-8B51-0574F9EA6CD4";

            //3df94231-87a0-40c5-812d-8ade73691f36 - SVG Viewer
            public static readonly Guid SVGViewer = new Guid("3DF94231-87A0-40C5-812D-8ADE73691F36");
            public const string SVGViewerIdString = "3DF94231-87A0-40C5-812D-8ADE73691F36";

            //07019810-AD30-4ECB-B577-200C0BFAAA5E  - RSS
            public static readonly Guid RSSBlock = new Guid("07019810-AD30-4ECB-B577-200C0BFAAA5E");
            public const string RSSBlockIdString = "07019810-AD30-4ECB-B577-200C0BFAAA5E";
        }

        public class G1GlueLayoutIDs
        {
            public static readonly Guid PageWithLeftNav = new Guid("b8cc18d8-1689-4ead-a934-f994619bf8be");
            public static readonly Guid PageWithoutLeftNav = new Guid("3298d377-0930-4bd1-9731-32585a0a0f65");
            public static readonly Guid StartPage = new Guid("be19d5ba-90bd-4cef-9d45-581362b75a75");
            public static readonly Guid NewsArticle = new Guid("6cf7e43b-80d6-4597-9080-8d8969c23311");
        }

        public class G1GlueLayoutZoneIDs
        {
            public const string RightZone = "rightzone";
            public const string MainZone = "mainzone";
            public const string Zone1 = "zone1";
            public const string Zone2 = "zone2";
            public const string Zone3 = "zone3";
            public const string Zone4 = "zone4";
            public const string Zone5 = "zone5";
            
        }

        public class G1FeatureIds
        {
            public static readonly Guid SpfxInfrastructure = new Guid("12ea6a5a-a2d8-4811-b267-553e97738f0c");

            public static readonly Guid CoreMasterPage = new Guid("e6c0ce55-bac6-4d45-97fb-972fd70bf8e7");

            public static readonly Guid TeamSitePrerequisites = new Guid("16dc1545-5bd8-46ce-9daf-b2f51fe6853e");

            public static readonly Guid OmniaDocumentManagementAuthoringInfrastructure = new Guid("b1ca6326-a8ec-475c-a389-88c23c8bfbcd");

            public static readonly Guid OmniaDocumentManagementAuthoringSite = new Guid("62d105f9-1ae9-4da2-970f-7b82260e5eaf");
            public static readonly Guid OmniaIntranetTeamSiteAnnouncements = new Guid("9bea2863-883c-46f7-be9a-652f0eedd340");
            public static readonly Guid OmniaIntranetTeamSiteCalendar = new Guid("f4ec666e-6fd6-4024-b47a-8ed5ea1168b4");
            public static readonly Guid OmniaIntranetTeamSiteLinks = new Guid("391f26b7-409a-4e2c-8fe7-96513155fb24");
            public static readonly Guid OmniaIntranetTeamSiteTasks = new Guid("b348a95b-9163-4d62-88e3-32fc1df5729e");
            public static readonly Guid OmniaDocumentManagementCreateDocumentWizard = new Guid("a4473bea-2093-41c4-b6a5-8c69a9be3a22");


        }

        public class G1BuiltInProperties
        {
            public const string SiteTemplateId = "pfp_sitetemplate_id";            
            public const string IsPublic = "omf_public";
        }

        public class BuiltInEnterpriseProperties
        {
            public static readonly string CreatedBy = "createdby";
            public static readonly string CreatedByUpperCase = "CreatedBy";
            public static readonly string CreatedAt = "createdat";
            public static readonly string CreatedAtUpperCase = "CreatedAt";
            public static readonly string CreatedAtUpperCase2 = "createdAt";
            public static readonly string ModifiedBy = "modifiedby";
            public static readonly string ModifiedByUpperCase = "ModifiedBy";
            public static readonly string ModifiedAt = "modifiedat";
            public static readonly string ModifiedAtUpperCase = "ModifiedAt";
            public static readonly string ModifiedAtUpperCase2 = "modifiedAt";

            public static readonly string PageImage = "owcmpageimage";

            public static readonly string PageContent = "owcmpagecontent";

            public static readonly string PageSummary = "owcmpagesummary";

            public static readonly string ArticleDate = "owcmarticledate";

            public static readonly string RelatedLinks = "owcmpagerelatedlinks";

            public static readonly string RelatedLinks2 = "owcmrelatedlinks";

            public static readonly string PeoplePreferredName = "panpreferredname";

            // Events
            public static readonly string EventStartDate = "omemstartdate";
            public static readonly string EventEndDate = "omemenddate";
            public static readonly string EventMaxParticipants = "omemmaximumparticipants";
            public static readonly string EventRegistrationStartDate = "omemregistrationstartdate";
            public static readonly string EventRegistrationEndDate = "omemregistrationenddate";
            public static readonly string EventCancellationEndDate = "omemcancellationenddate";
            public static readonly string EventIsOnlineMeeting = "omemisonlinemeeting";            
            public static readonly string EventOnlineMeetingUrl = "omemonlinemeetingurl";
            public static readonly string EventIsReservationOnly = "omemreservationonly";
            public static readonly string EventEnableStandbyList = "EventEnableStandbyList";
            public static readonly string EventIsSignUpColleague = "omemiscolleague";
            public static readonly string EventLocation = "EventLocation";
        }

        public class G1BlockViewIDs
        {
            public class NewsViewer
            {
                public const string SimpleListView = "omi-newsviewer-simplelisting-view";

                public const string ListViewV2 = "omi-newsviewer-listing-view-v2";
            }
        }

        public class BlockViewIDs
        {
            public class PageRollup
            {
                public static readonly Guid ListView = new Guid("7fffea21-6a1e-4971-bed5-d1b593a3e987");
                public const string ListViewIdString = "7fffea21-6a1e-4971-bed5-d1b593a3e987";

                public static readonly Guid ListViewWithImage = new Guid("7aaab7c9-75a7-4da8-a72c-106b489acbe1");
                public const string ListViewWithImageIdString = "7aaab7c9-75a7-4da8-a72c-106b489acbe1";

                public static readonly Guid RollerView = new Guid("a1a1c15a-9a7c-4067-abe6-850cca4caa15");
                public const string RollerViewIdString = "a1a1c15a-9a7c-4067-abe6-850cca4caa15";
            }
            public class RSSViewType
            {
                public static readonly Guid ImageOnLeft = new Guid("a18a9040-e115-4acc-8229-58dce0c6146f");
                public const string ImageOnLeftIdString = "a18a9040-e115-4acc-8229-58dce0c6146f";

                public static readonly Guid ImageOnRight = new Guid("d9aca318-983a-4841-9789-8d0078e74564");
                public const string ImageOnRightIdString = "d9aca318-983a-4841-9789-8d0078e74564";

                public static readonly Guid NoImageDisplayed = new Guid("b4c8c2ef-920b-4a94-8560-31ff1a0f2f17");
                public const string NoImageIdString = "b4c8c2ef-920b-4a94-8560-31ff1a0f2f17";
            }
        }

        public class AppDefinitionIDs
        { 
            public static readonly Guid TeamCollaborationDefinitionID = new Guid("d2240d7b-af3c-428c-bae8-5b8bfc08e3ac");
        }

        public class Configurations
        {
            public class SiteTemplatesMapping
            {
                public const string O365GroupDefaultMapping = "ff000000-0000-ffff-0000-0000000000ff";
            }
        }

        public class TeamWorkAppType
        {
            public const int SharePointTeamSite = 1;
            public const int Office365Group = 2;
            public const int YammerGroup = 3;
            public const int FacebookWorkplaceGroup = 4;
            public const int SharePointCommunicationSite = 5;
            public const int MicrosoftTeam = 6;
        }

    }
}

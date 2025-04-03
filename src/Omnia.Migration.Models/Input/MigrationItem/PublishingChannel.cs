using Omnia.Fx.Models.TargetingFilter;
using System;
using System.Collections.Generic;

namespace Omnia.Migration.Models.Input.MigrationItem
{
    public class ImportPublishingChannelObject
    {
        public List<PublishingChannelCategory> ChannelCategories { get; set; }
        public List<PublishingChannel> Channels { get; set; }
    }

    public class PublishingChannelCategory
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool? BuiltIn { get; set; }
        public int? Order { get; set; }
    }

    public class PublishingChannel
    {
        public Guid Uid { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PublicUrl { get; set; }
        public Guid? PublishingCategoryId { get; set; }
        public IList<string> OwnerEmails { get; set; }
        public IList<string> AdministratorEmails { get; set; }
        public IList<string> PublisherEmails { get; set; }
        public PublishingChannelImage Image { get; set; }
        public TargetingFilterData TargetingFilter { get; set; }
    }

    public class PublishingChannelImage
    {
        public string Path { get; set; }
        public string AltText { get; set; }
    }

    public class PageChannel
    {
        public string ChannelId { get; set; }
        public bool? IsDefault { get; set; }
    }
}

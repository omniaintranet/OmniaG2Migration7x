using Microsoft.Extensions.Options;
using Omnia.Fx.Models.Identities;
using Omnia.Fx.Models.Language;
using Omnia.Fx.Models.MediaPicker;
using Omnia.Fx.Models.Queries;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Mappers;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Omnia.Migration.Core.Services
{
    public class PublishingChannelService
    {
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private PublishingChannelApiHttpClient PublishingChannelApiHttpClient { get; }
        private WcmImageApiHttpClient ImageApiHttpClient { get; }

        public PublishingChannelService(IOptionsSnapshot<MigrationSettings> migrationSettings, PublishingChannelApiHttpClient publishingChannelApiHttpClient, WcmImageApiHttpClient imageApiHttpClient)
        {
            MigrationSettings = migrationSettings;
            PublishingChannelApiHttpClient = publishingChannelApiHttpClient;
            ImageApiHttpClient = imageApiHttpClient;
        }

        public async Task EnsureChannelCategoriesAsync(List<PublishingChannelCategory> channelCategories, LanguageTag defaultLanguage)
        {
            var existingChannelCategories = await PublishingChannelApiHttpClient.GetAllChannelCategoriesAsync();
            foreach (var category in channelCategories)
            {
                if (!existingChannelCategories.Any(c => c.Id == category.Id))
                {
                    var newCategory = new WebContentManagement.Models.ChannelManagement.PublishingChannelCategory()
                    {
                        Id = category.Id,
                        Title = new MultilingualString { { defaultLanguage, category.Title } },
                        BuiltIn = category.BuiltIn.HasValue ? category.BuiltIn.Value : false,
                        Order = category.Order.HasValue ? category.Order.Value : 0
                    };

                    var createCategoryResult = await PublishingChannelApiHttpClient.CreateChannelCategoryAsync(newCategory);
                    createCategoryResult.EnsureSuccessCode();
                }
            }
        }

        public async Task EnsureChannelsAsync(List<PublishingChannel> channels, LanguageTag defaultLanguage, ItemQueryResult<IResolvedIdentity> identities, Identity currentUser)
        {
            foreach (var channel in channels)
            {
                if (channel.Id > 0) continue;

                var owners = new List<Identity>();
                foreach (var ownerEmail in channel.OwnerEmails.Distinct())
                {
                    owners.Add(UserMaper.GetIdentitybyEmail(identities, ownerEmail));
                }

                var administrators = new List<UserIdentity>();
                foreach (var adminEmail in channel.AdministratorEmails.Distinct())
                {
                    administrators.Add(UserMaper.GetIdentitybyEmail(identities, adminEmail) as UserIdentity);
                }

                var publishers = new List<Identity>();
                foreach (var publisherEmail in channel.PublisherEmails.Distinct())
                {
                    publishers.Add(UserMaper.GetIdentitybyEmail(identities, publisherEmail));
                }

                // At least 1 user need to be added as Owner of channel
                if (owners.Count == 0 && currentUser != null)
                {
                    owners.Add(currentUser);
                }

                // At least 1 user need to be added as Administrator of channel
                if (administrators.Count == 0 && currentUser != null)
                {
                    administrators.Add(currentUser as UserIdentity);
                }

                // For publishing page to channel, Current User need to have Administrator or Publisher role
                if (currentUser != null && !publishers.Contains(currentUser) && !administrators.Contains(currentUser as UserIdentity))
                {
                    publishers.Add(currentUser);
                }

                MediaPickerImage channelImage = null;
                if (channel.Image != null)
                {
                    var inputPath = Path.Combine(MigrationSettings.Value.InputPath, channel.Image.Path);
                    var bytes = File.ReadAllBytes(inputPath);
                    string fileContent = Convert.ToBase64String(bytes);

                    channelImage = await ImageApiHttpClient.UploadChannelImageAsync(fileContent, Path.GetFileName(channel.Image.Path), channel.Image.AltText);
                }

                var createRequest = new WebContentManagement.Models.ChannelManagement.ChannelCreateRequest()
                {
                    Title = new MultilingualString { { defaultLanguage, channel.Title } },
                    Description = new MultilingualString { { defaultLanguage, channel.Description } },
                    PublishingCategoryId = channel.PublishingCategoryId.HasValue && channel.PublishingCategoryId.Value != Guid.Empty ? channel.PublishingCategoryId.Value : null,
                    PublicUrl = string.IsNullOrEmpty(channel.PublicUrl) ? "" : channel.PublicUrl,
                    Owner = owners,
                    Administrator = administrators,
                    Publisher = publishers,
                    Image = channelImage
                };

                var createChannelResult = await PublishingChannelApiHttpClient.CreatePublishingChannelAsync(createRequest);
                createChannelResult.EnsureSuccessCode();

                channel.Id = createChannelResult.Data.Id;
            }
        }

        public async Task PublishPageToChannelsAsync(int pageId, List<PageChannel> pageChannels, List<PublishingChannel> publishingChannels)
        {
            int defaultPublishingChannelId = 0;
            List<int> publishingChannelIds = new List<int>();
            foreach (var pageChannel in pageChannels)
            {
                int publishingChannelId = 0;
                if (!int.TryParse(pageChannel.ChannelId, out publishingChannelId))
                {
                    Guid channelUid = Guid.Empty;
                    if (Guid.TryParse(pageChannel.ChannelId, out channelUid))
                    {
                        var foundChannel = publishingChannels.FirstOrDefault(c => c.Uid == channelUid);
                        if (foundChannel != null)
                        {
                            publishingChannelId = foundChannel.Id;
                        }
                    }
                }

                if (publishingChannelId > 0)
                {
                    publishingChannelIds.Add(publishingChannelId);
                }

                if (defaultPublishingChannelId == 0 || (pageChannel.IsDefault.HasValue && pageChannel.IsDefault.Value))
                {
                    defaultPublishingChannelId = publishingChannelId;
                }
            }

            var publishToChannelsResult = await PublishingChannelApiHttpClient.PublishPageToChannelsAsync(pageId, publishingChannelIds, defaultPublishingChannelId);
            publishToChannelsResult.EnsureSuccessCode();
        }
    }
}

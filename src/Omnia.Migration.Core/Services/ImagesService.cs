using Omnia.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Omnia.Migration.Core.Helpers;
using Omnia.Migration.Core.Http;
using Omnia.Migration.Core.Reports;
using Omnia.Migration.Models.Configuration;
using Omnia.Migration.Models.Input.MigrationItem;
using Omnia.WebContentManagement.Models.Pages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks; 

namespace Omnia.Migration.Core.Services
{
    public class ImagesService
    {
        private bool imageNotFound { get; set; }
        private IHttpImageClient imageHttpClient { get; }
        private WcmImageApiHttpClient ImageApiHttpClient { get; }
        private IOptionsSnapshot<MigrationSettings> MigrationSettings { get; }
        private SPTokenService SPTokenService { get; }
        public ImagesService(
            SharePointImageHttpClient sharePointImageHttpClient,
            CustomHttpImageClient customHttpImageClient,
            WcmImageApiHttpClient imageApiHttpClient,
            SPTokenService spTokenService,
            IOptionsSnapshot<MigrationSettings> migrationSettings)
        {
            ImageApiHttpClient = imageApiHttpClient;
            MigrationSettings = migrationSettings;
            SPTokenService = spTokenService;
            if (MigrationSettings.Value.UseCustomImageClient)
            {
                imageHttpClient = customHttpImageClient;
            }
            else
            {
                imageHttpClient = sharePointImageHttpClient;
            }
        }

        public async Task MigrateImagesAsync(CheckedOutVersionPageData<PageData> checkoutVersion, ImportPagesReport importPagesReport, PageNavigationMigrationItem migrationItem)
        {
            var sharepointUrl = MigrationSettings.Value.WCMContextSettings.SharePointUrl.ToLower();
            var enterpriseProperties = checkoutVersion.PageData.EnterpriseProperties;
            var blocksData = checkoutVersion.PageData.PropertyBag.Where(x => x.Key.Contains("blockprop")).ToDictionary(x => x.Key, x => x.Value);

            var migratedImages = new Dictionary<string, string>();

            var propKeys = enterpriseProperties.Keys.ToList();
            foreach (var propKey in propKeys)
            {
                if (enterpriseProperties[propKey] == null || propKey == Constants.BuiltInEnterpriseProperties.RelatedLinks) //Diem - 22 Jul 22: do not migrate image URL in related links
                    continue;
                imageNotFound = false;
                var propValueStr = enterpriseProperties[propKey].ToString();
                var newPropValueStr = await MigrateAndReplaceImageUrlsAsync(propKey, propValueStr, checkoutVersion.PageId, sharepointUrl, migratedImages, importPagesReport, migrationItem);
                
                if (propValueStr != newPropValueStr)
                {
                    enterpriseProperties[propKey] = JsonHelper.SafeJTokenParse(newPropValueStr);
                }
                else if (propKey == "owcmpageimage" && imageNotFound)//21-06-2022: diem - set pageimage is null if image not found or can not get in g1
                {                    
                    enterpriseProperties[propKey] = null;
                }
            }

            var blockKeys = blocksData.Keys.ToList();
            foreach (var blockKey in blockKeys)
            {
                var blockDataStr = JsonConvert.SerializeObject(blocksData[blockKey]);
                var newBlockDataStr = await MigrateAndReplaceImageUrlsAsync(blockKey.ToString(), blockDataStr, checkoutVersion.PageId, sharepointUrl, migratedImages, importPagesReport, migrationItem);

                if (blockDataStr != newBlockDataStr)
                {
                    var newBlockData = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JToken>(newBlockDataStr);
                    checkoutVersion.PageData.PropertyBag[blockKey] = newBlockData;
                    //blocksData[blockKey] = newBlockData;
                }
            }

            checkoutVersion.PageData.EnterpriseProperties = enterpriseProperties;

            //checkoutVersion.PageData.LayoutData.BlockData = blocksData;
        }
        class UrlMap
        {
            public string Url { get; set; }
            public string Path { get; set; }
        }
        private async ValueTask<string> MigrateAndReplaceImageUrlsAsync(string key, string content, int pageId, string sharepointUrl, Dictionary<string, string> migratedImages, ImportPagesReport importPagesReport, PageNavigationMigrationItem migrationItem)
        {

            var imageSrcs = HtmlParser.ParseAllImageUrls(content);

            if (imageSrcs.Count == 0)
                return content;

            foreach (var imageSrc in imageSrcs)
            {
                try
                {
                    if (migratedImages.ContainsKey(imageSrc))
                    {
                        content = content.Replace(imageSrc, migratedImages[imageSrc]);
                    }
                    else if (imageSrc.ToLower().StartsWith(sharepointUrl) || imageSrc.ToLower().StartsWith("https://employee-xp.com"))
                    {
                        var imageFileName = Path.GetFileName(imageSrc).Split("?")[0];
                        var imageContent = await imageHttpClient.GetImage(imageSrc);                            
                        var size = imageContent.Length / 1024;
                        while (size > 10000)
                        {
                            //Reduce image size                            
                            imageContent = ReduceImageSize(imageContent);
                            size = imageContent.Length / 1024;
                        }

                        var base64 = Convert.ToBase64String(imageContent);
                        try
                        {
                            var svgContent = JsonConvert.DeserializeObject<Models.BlockData.SVGViewerData>(content);
                            if (svgContent.documentUrl != null)
                            {                               
                                var imgParts = imageSrc.Split("/");
                                string imgPath = imgParts[0] + "//" + imgParts[2] + "/" + imgParts[3] + "/" + imgParts[4]; 
                                svgContent.name = imageFileName.Split(".svg").First();
                                svgContent.spWebUrl = imgPath;
                                content = JsonConvert.SerializeObject(svgContent);
                                continue;
                            }
                        }
                        catch { }  

                        var newImageSrcResult = await ImageApiHttpClient.UploadPageImageAsync(base64, pageId, imageFileName); 
                        content= LinkParser.ImageContent(key,content,imageSrc,newImageSrcResult);
                        var configParamStartIndex = content.IndexOf("%22%2c%22configuration%22");
                        var configParamEndIndex = content.IndexOf("Video%22%3afalse%7d");
                        if (configParamStartIndex > -1 && configParamEndIndex > configParamStartIndex)
                        {
                            var configParam = content.Substring(configParamStartIndex, configParamEndIndex - configParamStartIndex + "Video%22%3afalse%7d".Length);
                            content = content.Replace(configParam, string.Empty);
                        }

                        migratedImages.Add(imageSrc, newImageSrcResult);
                    }
                }
                catch (Exception ex)
                {
                    if (key == "owcmpageimage" && ex.Message.Contains("Response status: NotFound"))
                    {
                        imageNotFound = true;
                    }
                    //throw ex;
                    ImportPagesReport.Instance.AddFailedItem(migrationItem, 99999999, pageId, imageSrc, ex);
                }
            }

            var videoSrcs = LinkParser.ParseIframeSrc(content);
            foreach ( var videoSrc in videoSrcs )
            {
                content = LinkParser.VideoContent(content, videoSrc);
            }

            return content;
        }
        public byte[] ReduceImageSize(byte[] inputBytes)
        {
            Byte[] outputBytes;
            var jpegQuality = 80;
            Image image;
            using (var inputStream = new MemoryStream(inputBytes))
            {
                image = Image.FromStream(inputStream);
                var jpegEncoder = ImageCodecInfo.GetImageDecoders()
                  .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);

                using (var outputStream = new MemoryStream())
                {
                    image.Save(outputStream, jpegEncoder, encoderParameters);
                    outputBytes = outputStream.ToArray();
                }
            }
            return outputBytes;
        }       
    }
}

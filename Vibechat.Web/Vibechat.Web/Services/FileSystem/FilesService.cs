using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Services.Images;
using Vibechat.Web.Services.Paths;
using VibeChat.Web;

namespace Vibechat.Web.Services.FileSystem
{
    public class FilesService : AbstractFilesService
    {
        private static int MessageImageMaxWidth = 350;

        private static int MessageImageMaxHeight = 350;

        private static int ThumbnailHeight = 140;

        private static int ThumbnailWidth = 140;

        private static string FullSized = "full";

        private static string Compressed = "cmpr";

        private static int MaxFileNameLength = 120;

        public FilesService(
            IImageScalingService imageScaling,
            UniquePathsProvider pathsProvider,
            IImageCompressionService imageCompression) : base(pathsProvider)
        {
            ImageScaling = imageScaling;
            ImageCompression = imageCompression;
        }

        public IImageScalingService ImageScaling { get; }
        public IImageCompressionService ImageCompression { get; }

        /// <summary>
        /// Takes stream containing image and returns <see cref="MessageAttachment"/> object.
        /// This method doesn't compress image, it just calculates scaled dimensions
        /// And saves the image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public async Task<MessageAttachment> SaveMessagePicture(IFormFile formFile, MemoryStream image, string imageName, string chatOrUserId, string sender)
        {
            try
            {
                ValueTuple<int, int> resultDimensions = ImageScaling.GetScaledDimensions(image, MessageImageMaxWidth, MessageImageMaxHeight);

                image.Seek(0, SeekOrigin.Begin);

                imageName = imageName.Length > MaxFileNameLength ? imageName.Substring(0, MaxFileNameLength) : imageName;

                var resultPath = await base.SaveFile(formFile, image, imageName, chatOrUserId, sender);

                return new MessageAttachment()
                {
                    AttachmentKind = "img",
                    AttachmentName = imageName,
                    ContentUrl = DI.Configuration["FileServer:Url"] + resultPath.Substring(StaticFilesLocation.Length),
                    ImageHeight = resultDimensions.Item2,
                    ImageWidth = resultDimensions.Item1,
                    FileSize = image.Length
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
        }

        public async Task<MessageAttachment> SaveFile(IFormFile formFile, MemoryStream file, string filename, string chatOrUserId, string sender)
        {
            try
            {
                filename = filename.Length > MaxFileNameLength ? filename.Substring(0, MaxFileNameLength) : filename;

                var resultPath = await base.SaveFile(formFile, file, filename, chatOrUserId, sender);

                return new MessageAttachment()
                {
                    AttachmentKind = "file",
                    AttachmentName = filename,
                    ContentUrl = DI.Configuration["FileServer:Url"] + resultPath.Substring(StaticFilesLocation.Length),
                    FileSize = file.Length
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload this file.", ex);
            }
        }

        /// <summary>
        /// Returns new thumbnail url and full-sized image url.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public async Task<ValueTuple<string,string>> SaveProfileOrChatPicture(IFormFile formFile, MemoryStream image, string imageName, string chatOrUserId, string sender)
        {
            try
            {
                var resized = ImageCompression.Resize(image, ThumbnailWidth, ThumbnailHeight);

                resized.Seek(0, SeekOrigin.Begin);
                image.Seek(0, SeekOrigin.Begin);

                imageName = imageName.Length > MaxFileNameLength ? imageName.Substring(0, MaxFileNameLength) : imageName;

                var uncompressedFileName = await base.SaveFile(formFile, image, imageName, chatOrUserId, sender, FullSized);

                var compressedFileName = await base.SaveFile(formFile, resized, imageName, chatOrUserId, sender, Compressed,
                    Path.GetDirectoryName(uncompressedFileName) + Path.DirectorySeparatorChar);

                resized.Dispose();

                return new ValueTuple<string, string>(
                    DI.Configuration["FileServer:Url"] + compressedFileName.Substring(StaticFilesLocation.Length),
                    DI.Configuration["FileServer:Url"] + uncompressedFileName.Substring(StaticFilesLocation.Length));
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
        }

        public override async Task SaveToStorage(IFormFile formFile, MemoryStream file, string path)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(DI.Configuration["FileServer:Url"]);

                var fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(formFile.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = file.Length,
                            ContentType = new MediaTypeHeaderValue(formFile.ContentType)
                        }
                    }, "file", fileName);

                    content.Add(new StringContent(path), "path");

                    //server responds with either true or false.
                    HttpResponseMessage response = await client.PostAsync(DI.Configuration["FileServer:UploadFileUrl"], content);

                    if (!bool.Parse(await response.Content.ReadAsStringAsync()))
                    {
                        throw new InvalidDataException("Failed to upload this file.");
                    }
                }
            }
        }
    }
}

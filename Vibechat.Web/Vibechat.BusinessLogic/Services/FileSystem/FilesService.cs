using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vibechat.BusinessLogic.Services.Images;
using Vibechat.BusinessLogic.Services.Paths;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.BusinessLogic.Services.FileSystem
{
    public class FilesService : AbstractFilesService
    {
        private const int MessageImageMaxWidth = 800;

        private const int MessageImageMaxHeight = 800;

        private const int ThumbnailHeight = 140;

        private const int ThumbnailWidth = 140;

        private const string FullSized = "full";

        private const string Compressed = "cmpr";

        private const int MaxFileNameLength = 120;

        private readonly ILogger<FilesService> logger;

        private readonly IConfiguration config;

        public FilesService(
            IImageScalingService imageScaling,
            UniquePathsProvider pathsProvider,
            IImageCompressionService imageCompression,
            ILogger<FilesService> logger,
            IHttpClientFactory httpClientFactory,
            IConfiguration config) : base(pathsProvider)
        {
            ImageScaling = imageScaling;
            ImageCompression = imageCompression;
            this.logger = logger;
            this.config = config;
            HttpClient = httpClientFactory.CreateClient();
        }

        private IImageScalingService ImageScaling { get; }
        private IImageCompressionService ImageCompression { get; }
        
        private HttpClient HttpClient { get; set; }

        /// <summary>
        ///     Takes stream containing image and returns <see cref="Attachment" /> object.
        ///     This method doesn't compress image, it just calculates scaled dimensions
        ///     And saves the image.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="imageName"></param>
        /// <param name="chatOrUserId"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<MessageAttachment> SaveMessagePicture(IFormFile formFile,
            string imageName, string chatOrUserId, string sender)
        {
            try
            {
                using (var image = new MemoryStream())
                {
                    formFile.CopyTo(image);
                    image.Seek(0, SeekOrigin.Begin);
                    
                    var resultDimensions =
                        ImageScaling.GetScaledDimensions(image, MessageImageMaxWidth, MessageImageMaxHeight);

                    image.Seek(0, SeekOrigin.Begin);

                    var resized = ImageCompression.Resize(image, resultDimensions.Item1, resultDimensions.Item2);

                    var imageNameWithoutExt = Path.GetFileNameWithoutExtension(imageName);

                    var extension = Path.GetExtension(imageName);

                    imageName = imageName.Length > MaxFileNameLength
                        ? imageNameWithoutExt.Substring(0, MaxFileNameLength - extension.Length) + extension
                        : imageName;

                    var resultPath = await SaveFile(formFile, resized, imageName, chatOrUserId, sender);

                    return new MessageAttachment
                    {
                        AttachmentKind = AttachmentKind.Image,
                        AttachmentName = imageName,
                        ContentUrl = config["FileServer:Url"] + resultPath,
                        ImageHeight = resultDimensions.Item2,
                        ImageWidth = resultDimensions.Item1
                    };   
                }
            }
            catch (ArgumentException e)
            {
                logger.LogError(e, "While resizing an image.");

                throw new Exception("Failed to upload this image.", e);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "While resizing an image.");
                throw new Exception("Failed to upload this image.", ex);
            }
        }

        public async Task<MessageAttachment> SaveMessageFile(IFormFile formFile, string filename,
            string chatOrUserId, string sender)
        {
            try
            {
                using (var file = new MemoryStream())
                {
                    formFile.CopyTo(file);
                    file.Seek(0, SeekOrigin.Begin);
                    
                    filename = filename.Length > MaxFileNameLength ? filename.Substring(0, MaxFileNameLength) : filename;

                    var resultPath = await SaveFile(formFile, file, filename, chatOrUserId, sender);

                    return new MessageAttachment
                    {
                        AttachmentKind = AttachmentKind.File,
                        AttachmentName = filename,
                        ContentUrl = config["FileServer:Url"] + resultPath,
                        FileSize = file.Length
                    };
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
        }

        /// <summary>
        ///     Returns new thumbnail url and full-sized image url.
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="imageName"></param>
        /// <param name="chatOrUserId"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<ValueTuple<string, string>> SaveProfileOrChatPicture(IFormFile formFile,
            string imageName, string chatOrUserId, string sender)
        {
            try
            {
                using (var image = new MemoryStream())
                {
                    formFile.CopyTo(image);
                    image.Seek(0, SeekOrigin.Begin);
                    
                    var resized = ImageCompression.Resize(image, ThumbnailWidth, ThumbnailHeight);

                    resized.Seek(0, SeekOrigin.Begin);
                    image.Seek(0, SeekOrigin.Begin);

                    imageName = imageName.Length > MaxFileNameLength
                        ? imageName.Substring(0, MaxFileNameLength)
                        : imageName;

                    var uncompressedFileName = await SaveFile(formFile, image, imageName, chatOrUserId, sender, FullSized);

                    var compressedFileName = await SaveFile(formFile, resized, imageName, chatOrUserId, sender, Compressed,
                        Path.GetDirectoryName(uncompressedFileName) + Path.DirectorySeparatorChar);

                    resized.Dispose();

                    return new ValueTuple<string, string>(
                        config["FileServer:Url"] + compressedFileName,
                        config["FileServer:Url"] + uncompressedFileName);
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload this image.", ex);
            }
        }

        public void DeleteFiles(List<string> paths)
        {
            Parallel.ForEach(paths, async path =>
            {
                try
                {
                    await DeleteFile(path);
                }
                catch 
                {
                    //exception is already logged, do nothing.
                }
            });
        }

        protected override async Task DeleteFile(string path)
        {
            try
            {
                await HttpClient.DeleteAsync(config["FileServer:DeleteFileUrl"] + path);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete file!");
                throw new HttpRequestException("Failed to delete the file.", e);
            }
        }
        
        protected override async Task SaveToStorage(IFormFile formFile, MemoryStream file, string path)
        {
            var fileName = ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');

            var content = new MultipartFormDataContent
            {
                {
                    new StreamContent(file)
                    {
                        Headers =
                        {
                            ContentLength = file.Length,
                            ContentType = new MediaTypeHeaderValue(formFile.ContentType)
                        }
                    },
                    "file",
                    fileName
                },

                {new StringContent(path), "path"}
            };

            //server responds with either true or false.
            var response = await HttpClient.PostAsync(config["FileServer:UploadFileUrl"], content);

            if (!bool.TryParse(await response.Content.ReadAsStringAsync(), out var result))
            {
                throw new InvalidDataException("Failed to upload this file.");
            }

            if (!result)
            {
                throw new InvalidDataException("Failed to upload this file.");
            }
        }
    }
}
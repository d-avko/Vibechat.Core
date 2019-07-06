using System;
using System.IO;
using System.Text;
using Vibechat.Web.ChatData.Messages;
using Vibechat.Web.Services.Images;
using Vibechat.Web.Services.Paths;

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
        public MessageAttachment SaveMessagePicture(MemoryStream image, string imageName, string chatOrUserId, string sender)
        {
            ValueTuple<int, int> resultDimensions = ImageScaling.GetScaledDimensions(image, MessageImageMaxWidth, MessageImageMaxHeight);

            image.Seek(0, SeekOrigin.Begin);

            imageName = imageName.Length > MaxFileNameLength ? imageName.Substring(0, MaxFileNameLength) : imageName;

            var resultPath = base.SaveFile(image, imageName, chatOrUserId, sender);

            return new MessageAttachment()
            {
                AttachmentKind = "img",
                AttachmentName = imageName,
                ContentUrl = resultPath.Substring(StaticFilesLocation.Length),
                ImageHeight = resultDimensions.Item2,
                ImageWidth = resultDimensions.Item1,
                FileSize = image.Length
            };
        }

        public MessageAttachment SaveFile(MemoryStream file, string filename, string chatOrUserId, string sender)
        {
            filename = filename.Length > MaxFileNameLength ? filename.Substring(0, MaxFileNameLength) : filename;

            var resultPath = base.SaveFile(file, filename, chatOrUserId, sender);

            return new MessageAttachment()
            {
                AttachmentKind = "file",
                AttachmentName = filename,
                ContentUrl = resultPath.Substring(StaticFilesLocation.Length),
                FileSize = file.Length
            };
        }

        /// <summary>
        /// Returns new thumbnail url and full-sized image url.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public ValueTuple<string,string> SaveProfileOrChatPicture(MemoryStream image, string imageName, string chatOrUserId, string sender)
        {
            var resized = ImageCompression.Resize(image, ThumbnailWidth, ThumbnailHeight);

            resized.Seek(0, SeekOrigin.Begin);

            imageName = imageName.Length > MaxFileNameLength ? imageName.Substring(0, MaxFileNameLength) : imageName;

            var uncompressedFileName = base.SaveFile(image, imageName, chatOrUserId, sender, FullSized);

            var compressedFileName = base.SaveFile(resized, imageName, chatOrUserId, sender, Compressed, 
                Path.GetDirectoryName(uncompressedFileName) + Path.DirectorySeparatorChar);

            resized.Dispose();

            return new ValueTuple<string, string>(compressedFileName.Substring(StaticFilesLocation.Length), uncompressedFileName.Substring(StaticFilesLocation.Length));
        }
    }
}

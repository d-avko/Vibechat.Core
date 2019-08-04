using System.IO;

namespace Vibechat.Web.Services.Images
{
    public interface IImageCompressionService
    {
        /// <summary>
        ///     Resize an image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns>Image, its width and height</returns>
        MemoryStream Resize(MemoryStream imageBytes, int scaleWidth, int scaleHeight);
    }
}
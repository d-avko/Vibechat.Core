using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace Vibechat.Web.Services.Images
{
    public class ImageCompressionService : IImageCompressionService, IImageScalingService
    {
        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns>Image, its width and height</returns>
        public Stream Resize(MemoryStream imageBytes, int scaleWidth, int scaleHeight)
        {
            SKCodec codec = SKCodec.Create(imageBytes);

            SKImageInfo info = codec.Info;

            SKImageInfo desired = new SKImageInfo(scaleWidth, scaleHeight);

            SKBitmap bmp = SKBitmap.Decode(codec, info);

            bmp = bmp.Resize(desired, SKFilterQuality.Medium);

            using (var image = SKImage.FromBitmap(bmp))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
            {
                var result = new MemoryStream();
                data.SaveTo(result);
                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
        }

        public ValueTuple<int, int> GetScaledDimensions(MemoryStream image, int maxWidth, int maxHeight)
        {
            SKCodec codec = SKCodec.Create(image);

            SKImageInfo info = codec.Info;

            int resultingWidth, resultingHeight;

            if (info.Width > info.Height)
            {
                resultingWidth = maxWidth;

                resultingHeight = (int)(maxWidth * (info.Height / (float)info.Width));
            }
            else if (info.Width < info.Height)
            {
                resultingHeight = maxHeight;

                resultingWidth = (int)(maxHeight * (info.Width / (float)info.Height));
            }
            else
            {
                resultingWidth = resultingHeight = maxHeight;
            }

            return new ValueTuple<int, int>(resultingWidth, resultingHeight);
        }
    }
}

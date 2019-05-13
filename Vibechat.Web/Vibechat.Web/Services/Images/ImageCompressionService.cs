using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace Vibechat.Web.Services.Images
{
    public class ImageCompressionService : IImageCompressionService
    {
        public static int MaxImageHeight = 350;

        public static int MaxImageWidth = 350;

        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns>Image, its width and height</returns>
        public ValueTuple<Stream, int, int> Resize(MemoryStream imageBytes)
        {
            SKCodec codec = SKCodec.Create(imageBytes);

            SKImageInfo info = codec.Info;

            int resultingWidth, resultingHeight;
            
            if(info.Width > info.Height)
            {
                resultingWidth = MaxImageWidth;

                resultingHeight = (int)(MaxImageWidth * (info.Height / (float)info.Width));
            }
            else if(info.Width < info.Height)
            {
                resultingHeight = MaxImageHeight;

                resultingWidth = (int)(MaxImageHeight * (info.Width / (float)info.Height));
            }
            else
            {
                resultingWidth = resultingHeight = MaxImageHeight;
            }

            SKImageInfo desired = new SKImageInfo(resultingWidth, resultingHeight);

            SKBitmap bmp = SKBitmap.Decode(codec, info);

            bmp = bmp.Resize(desired, SKBitmapResizeMethod.Lanczos3);

            using (var image = SKImage.FromBitmap(bmp))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
            {
                var result = new MemoryStream();
                data.SaveTo(result);
                result.Seek(0, SeekOrigin.Begin);
                return new ValueTuple<Stream, int, int>(result, bmp.Width, bmp.Height);
            }
        }
    }
}

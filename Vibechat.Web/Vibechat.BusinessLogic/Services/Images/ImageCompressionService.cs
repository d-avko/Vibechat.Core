using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Vibechat.BusinessLogic.Services.Images
{
    public class ImageCompressionService : IImageCompressionService, IImageScalingService
    {
        /// <summary>
        ///     Resize an image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns>Image, its width and height</returns>
        public MemoryStream Resize(MemoryStream imageBytes, int width, int height)
        {
            using (var image = new Bitmap(imageBytes))
            {
                var destRect = new Rectangle(0, 0, width, height);
                using (var destImage = new Bitmap(width, height))
                {
                    using (var graphics = Graphics.FromImage(destImage))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        using (var wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel,
                                wrapMode);
                        }
                    }

                    var resultStream = new MemoryStream();
                    destImage.Save(resultStream, ImageFormat.Jpeg);
                    resultStream.Seek(0, SeekOrigin.Begin);

                    return resultStream;
                }
            }
        }

        public ValueTuple<int, int> GetScaledDimensions(MemoryStream image, int maxWidth, int maxHeight)
        {
            using (var bitmap = new Bitmap(image))
            {
                int resultingWidth, resultingHeight;

                if (bitmap.Width > bitmap.Height)
                {
                    resultingWidth = maxWidth;

                    resultingHeight = (int) (maxWidth * (bitmap.Height / (float) bitmap.Width));
                }
                else if (bitmap.Width < bitmap.Height)
                {
                    resultingHeight = maxHeight;

                    resultingWidth = (int) (maxHeight * (bitmap.Width / (float) bitmap.Height));
                }
                else
                {
                    resultingWidth = resultingHeight = maxHeight;
                }

                return new ValueTuple<int, int>(resultingWidth, resultingHeight);
            }
        }
    }
}
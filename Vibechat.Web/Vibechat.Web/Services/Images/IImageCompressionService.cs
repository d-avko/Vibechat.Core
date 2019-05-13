using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.Images
{
    public interface IImageCompressionService
    {
        /// <summary>
        /// Resize an image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns>Image, its width and height</returns>
        ValueTuple<Stream, int, int> Resize(MemoryStream imageBytes);
    }
}

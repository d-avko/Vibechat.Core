using System;
using System.IO;

namespace Vibechat.Web.Services.Images
{
    public interface IImageScalingService
    {
        /// <summary>
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Width, Height</returns>
        ValueTuple<int, int> GetScaledDimensions(MemoryStream image, int maxWidth, int maxHeight);
    }
}
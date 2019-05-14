using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.Images
{
    public interface IImageScalingService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns>Width, Height</returns>
        ValueTuple<int, int> GetScaledDimensions(MemoryStream image);
    }
}

using System;
using System.IO;
using System.Text;
using Vibechat.Web.Services.Hashing;

namespace Vibechat.Web.Services.Paths
{
    public class UniquePathsProvider
    {
        public UniquePathsProvider(IHexHashingService hasher)
        {
            this.hasher = hasher;
        }

        private IHexHashingService hasher { get; }

        public string GetUniquePath(string randomizeValue)
        {
            return hasher.Hash(
                       Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffffK") +
                                              randomizeValue)) + Path.DirectorySeparatorChar;
        }
    }
}
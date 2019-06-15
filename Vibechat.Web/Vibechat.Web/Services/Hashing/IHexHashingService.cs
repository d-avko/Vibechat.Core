using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.Hashing
{
    public interface IHexHashingService
    {
        string Hash(byte[] value);

        string Hash(string value);
    }
}

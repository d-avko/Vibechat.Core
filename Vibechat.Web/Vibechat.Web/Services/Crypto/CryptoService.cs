using OpenSSL.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Conversations;
using Vibechat.Web.Services.Repositories;

namespace Vibechat.Web.Services.Crypto
{
    public class CryptoService
    {
        public CryptoService()
        {

        }

        public const int KeyLength = 2048;

        public DhPublicKey GenerateDhPublicKey()
        {
            var dh = new DH(KeyLength, DH.Generator5);
            return new DhPublicKey() { Generator = DH.Generator5.ToString(), Modulus = dh.P.ToDecimalString()};
        }
    }
}

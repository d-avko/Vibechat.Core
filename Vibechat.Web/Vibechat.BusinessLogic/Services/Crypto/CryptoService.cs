using OpenSSL.Crypto;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.BusinessLogic.Services.Crypto
{
    public class CryptoService
    {
        public const int KeyLength = 2048;

        public DhPublicKey GenerateDhPublicKey()
        {
            var dh = new DH(KeyLength, DH.Generator5);
            return new DhPublicKey {Generator = DH.Generator5.ToString(), Modulus = dh.P.ToDecimalString()};
        }
    }
}
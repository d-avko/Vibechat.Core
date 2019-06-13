using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Vibechat.Web.Services.Encryption
{
    public class EncryptionService
    {
        public EncryptionService()
        {
            
        }
        private const int AesKeyLength = 32;

        /// <summary>
        /// Encrypts data, using key and salt
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encryptionKey"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string Encrypt(string data, string encryptionKey, string salt)
        {
            var bytesDerivation = new Rfc2898DeriveBytes(encryptionKey, Convert.FromBase64String(salt));

            var KeyAndIv = bytesDerivation.GetBytes(64);

            using(var aes = Aes.Create())
            {
                Array.Copy(KeyAndIv, aes.Key, AesKeyLength);
                Array.Copy(KeyAndIv, AesKeyLength, aes.IV, 0, AesKeyLength);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }

                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }
    }
}

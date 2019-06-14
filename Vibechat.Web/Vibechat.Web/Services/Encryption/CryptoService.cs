using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Vibechat.Web.Services.Hashing;

namespace Vibechat.Web.Services.Encryption
{
    public class CryptoService
    {
        public CryptoService(IHexHashingService hashingService)
        {
            this.hashingService = hashingService;
        }
        private const int AesKeyLength = 32;

        private const int AesBlockSize = 32 * 8;

        private const int AesKeySize = 32 * 8;
        private readonly IHexHashingService hashingService;

        /// <summary>
        /// Encrypts data, using key and salt
        /// </summary>
        /// <param name="data">data to encode</param>
        /// <param name="encryptionKey">key to encrypt with</param>
        /// <param name="salt">hex string</param>
        /// <returns>data encrypted and encoded in Base64</returns>
        public string Encrypt(string data, string encryptionKey, string salt = null)
        {
            //use only first half of the key for encryption.

            encryptionKey = encryptionKey.Substring(0, encryptionKey.Length / 2);

            var bytesDerivation = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes(salt ?? GetSalt(encryptionKey, data)));

            byte[] KeyAndIv = bytesDerivation.GetBytes(64);

            using(var aes = Aes.Create())
            {
                aes.BlockSize = AesBlockSize;
                aes.KeySize = AesKeySize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

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

        public string Decrypt(string data, string encryptionKey, string salt)
        {
            encryptionKey = encryptionKey.Substring(0, encryptionKey.Length / 2);

            var bytesDerivation = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes(salt));

            byte[] KeyAndIv = bytesDerivation.GetBytes(64);
            byte[] binaryEncryptedData = Convert.FromBase64String(data);
            
            using (var aes = Aes.Create())
            {
                aes.BlockSize = AesBlockSize;
                aes.KeySize = AesKeySize;
                aes.Mode = CipherMode.CBC;

                Array.Copy(KeyAndIv, aes.Key, AesKeyLength);
                Array.Copy(KeyAndIv, AesKeyLength, aes.IV, 0, AesKeyLength);

                ICryptoTransform decryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        using (BinaryWriter swDecrypt = new BinaryWriter(csDecrypt))
                        {
                            swDecrypt.Write(binaryEncryptedData);
                        }

                        return Encoding.UTF8.GetString(msDecrypt.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// returns salt based on encryption key and data: SHA256(part of encryptionkey + data)
        /// </summary>
        /// <param name="encryptionKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public string GetSalt(string encryptionKey, string data)
        {
            return hashingService.Hash(encryptionKey.Substring(encryptionKey.Length / 2) + data);
        }
    }
}

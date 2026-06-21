using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CivicConnect.Web.Helpers
{
    public static class EncryptionHelper
    {
        // 256-bit key and 128-bit IV for AES
        // In a real application, this should be read from configuration/secrets
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("CIVIC_CONNECT_SECURE_KEY_32BYTES");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("CIVIC_CONNECT_IV");

        public static string? Encrypt(string? clearText)
        {
            if (string.IsNullOrEmpty(clearText)) return clearText;
            
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(clearText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string? Decrypt(string? cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            // Simple check to avoid crashing if plaintext is already in DB before encryption was added
            if (!cipherText.EndsWith("=") && !cipherText.Contains("+") && !cipherText.Contains("/"))
            {
                if (cipherText.Length < 20) return cipherText; // Likely plaintext citizen ID
            }

            try
            {
                var buffer = Convert.FromBase64String(cipherText);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    using (var msDecrypt = new MemoryStream(buffer))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Fallback to original if decryption fails (e.g., was plaintext)
                return cipherText;
            }
        }
    }
}

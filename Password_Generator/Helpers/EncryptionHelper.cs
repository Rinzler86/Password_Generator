using System.Security.Cryptography;
using System.Text;

namespace Password_Generator.Helpers
{
    public static class EncryptionHelper
    {
        // Ensure the key is exactly 32 bytes (256-bit) and the IV is 16 bytes (128-bit)
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 characters = 32 bytes
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 characters = 16 bytes

        public static string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;
                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                return sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // If decryption fails (for example if the value was not encrypted), return the original text.
                return cipherText;
            }
        }
    }
}

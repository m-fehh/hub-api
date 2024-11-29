using System.Security.Cryptography;
using System.Text;

namespace Hub.Infrastructure.Extensions.Generate
{
    public interface IStringEncrypter
    {
        string Decrypt(string cryptedString);
        string Encrypt(string originalString, bool useSafeUrl = false);
    }

    public class StringEncrypter : IStringEncrypter
    {
        static byte[] bytes = Encoding.UTF8.GetBytes("vndWasdk");
        public string Encrypt(string originalString, bool useSafeUrl = false)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException("The string which needs to be encrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();

            cryptoProvider.Mode = CipherMode.ECB;

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            var result = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            if (useSafeUrl == true)
            {
                result = result.Replace('+', '-').Replace('/', '_');
            }

            return result;
        }

        public string Decrypt(string cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
            {
                throw new ArgumentNullException("The string which needs to be decrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            cryptoProvider.Mode = CipherMode.ECB;

            MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}

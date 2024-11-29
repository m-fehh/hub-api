using System.Security.Cryptography;
using System.Text;

namespace Hub.Infrastructure.Extensions.Generate
{
    public interface IRandomGeneration
    {
        int Generate(int maxValue = 1000000);

        string GenerateString(int length = 10);

        string GenerateHashCode(int length);
    }

    internal class RandomGeneration : IRandomGeneration
    {
        private Random random = new Random();
        private RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();

        public int Generate(int maxValue = 1000000)
        {
            return random.Next(maxValue);
        }

        private int GenerateRandomInt(int length)
        {
            var byteArray = new byte[4];
            provider.GetBytes(byteArray);

            var randomInteger = Convert.ToInt64(BitConverter.ToUInt32(byteArray, 0));
            var result = (int)(randomInteger % 36);

            return result;
        }

        public string GenerateString(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length)

              .Select(s => s[GenerateRandomInt(s.Length)]).ToArray());
        }

        public string GenerateHashCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%&*()_=+-;:?/|";

            var randomString = new string(Enumerable.Repeat(chars, length)

              .Select(s => s[random.Next(s.Length)]).ToArray());

            using (var algo = new SHA512Managed())
            {
                return GenerateHashString(algo, randomString).Substring(0, length).ToUpper();
            }
        }
        private static string GenerateHashString(HashAlgorithm algo, string text)
        {
            // Compute hash from text parameter
            algo.ComputeHash(Encoding.UTF8.GetBytes(text));

            // Get has value in array of bytes
            var result = algo.Hash;

            // Return as hexadecimal string
            return string.Join(
                string.Empty,
                result.Select(x => x.ToString("x2")));
        }
    }
}

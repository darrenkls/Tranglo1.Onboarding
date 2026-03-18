using System.Security.Cryptography;
using System.Text;

namespace Tranglo1.Onboarding.Application.Helper
{
    public static class SecurityStringGenerator
    {
        public static string GenerateRandomString(int length)
        {
            // Define the pool of allowed characters
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                sb.Append(chars[RandomNumberGenerator.GetInt32(0, chars.Length)]);
            }

            return sb.ToString();
        }
    }
}

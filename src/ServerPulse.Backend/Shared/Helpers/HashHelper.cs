using IdentityModel;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Helpers
{
    public static class HashHelper
    {
        public static string ComputeHash(string str)
        {
            using var sha256 = SHA256.Create();
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            var codeChallenge = Base64Url.Encode(challengeBytes);
            return codeChallenge;
        }
    }
}

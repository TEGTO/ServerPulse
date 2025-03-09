using Authentication.Token;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Rsa
{
    internal interface IRsaKeyManager
    {
        public RsaSecurityKey PublicKey { get; }
        public RsaSecurityKey PrivateKey { get; }
        public void ReloadKeys(JwtSettings jwtSettings);
    }
}
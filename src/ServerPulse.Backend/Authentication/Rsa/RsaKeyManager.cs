using Authentication.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Authentication.Rsa
{
    internal sealed class RsaKeyManager : IRsaKeyManager
    {
        private RSA? privateRsa;
        private RSA? publicRsa;

        public RsaSecurityKey PublicKey
        {
            get
            {
                if (publicRsa == null)
                {
                    throw new ObjectDisposedException(nameof(publicRsa));
                }

                return new RsaSecurityKey(publicRsa);
            }
        }

        public RsaSecurityKey PrivateKey
        {
            get
            {
                if (privateRsa == null)
                {
                    throw new ObjectDisposedException(nameof(privateRsa));
                }

                return new RsaSecurityKey(privateRsa);
            }
        }

        public RsaKeyManager(IOptions<JwtSettings> options)
        {
            LoadKeys(options.Value);
        }

        ~RsaKeyManager()
        {
            DisposeKeys();
        }

        public void ReloadKeys(JwtSettings jwtSettings)
        {
            DisposeKeys();
            LoadKeys(jwtSettings);
        }

        private void LoadKeys(JwtSettings jwtSettings)
        {
            privateRsa = RSA.Create();
            privateRsa.ImportFromPem(jwtSettings.PrivateKey);

            publicRsa = RSA.Create();
            publicRsa.ImportFromPem(jwtSettings.PublicKey);
        }

        private void DisposeKeys()
        {
            privateRsa?.Dispose();
            publicRsa?.Dispose();
            privateRsa = null;
            publicRsa = null;
        }
    }
}
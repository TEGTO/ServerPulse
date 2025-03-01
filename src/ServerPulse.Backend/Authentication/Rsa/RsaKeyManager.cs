using Authentication.Token;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Authentication.Rsa
{
    internal sealed class RsaKeyManager : IRsaKeyManager
    {
        private RSA? privateRsa;
        private RSA? publicRsa;
        private readonly ILogger<RsaKeyManager> logger;

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

        public RsaKeyManager(IOptions<JwtSettings> options, ILogger<RsaKeyManager> logger)
        {
            this.logger = logger;

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
            if (!jwtSettings.PrivateKey.IsNullOrEmpty())
            {
                privateRsa = RSA.Create();
                privateRsa.ImportFromPem(jwtSettings.PrivateKey);
            }
            else
            {
                logger.LogInformation("No private key found in configuration.");
            }

            if (!jwtSettings.PublicKey.IsNullOrEmpty())
            {
                publicRsa = RSA.Create();
                publicRsa.ImportFromPem(jwtSettings.PublicKey);
            }
            else
            {
                logger.LogInformation("No public key found in configuration.");
            }
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
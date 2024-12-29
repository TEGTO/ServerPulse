namespace Authentication.Token
{
    public class JwtSettings
    {
        public const string SETTINGS_SECTION = "AuthSettings";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public double ExpiryInMinutes { get; set; }
    }
}

namespace Authentication.Token
{
    public class JwtSettings
    {
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required string PrivateKey { get; set; }
        public required string PublicKey { get; set; }
        public double ExpiryInMinutes { get; set; }
    }
}

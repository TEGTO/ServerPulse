namespace Authentication.Configuration
{
    public class JwtSettings
    {
        public virtual string Key { get; set; }
        public virtual string Issuer { get; set; }
        public virtual string Audience { get; set; }
        public virtual double ExpiryInMinutes { get; set; }
    }
}

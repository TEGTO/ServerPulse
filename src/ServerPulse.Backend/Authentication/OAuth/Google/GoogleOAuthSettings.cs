namespace Authentication.OAuth.Google
{
    public class GoogleOAuthSettings
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string Scope { get; set; }
    }
}

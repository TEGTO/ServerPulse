namespace AuthenticationApi.Dtos.OAuth
{
    public class GetOAuthUrlQueryParams
    {
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
    }
}

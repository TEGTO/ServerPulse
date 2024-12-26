namespace AuthenticationApi.Infrastructure.Models
{
    public record OAuthCodeModel
    {
        public required string Code { get; set; }
        public required string CodeVerifier { get; set; }
        public required string RedirectUrl { get; set; }
    }
}

namespace AuthenticationApi.Infrastructure.Models
{
    public record GenerateOAuthRequestUrlModel
    {
        public required string RedirectUrl { get; set; }
        public required string CodeVerifier { get; set; }
    }
}

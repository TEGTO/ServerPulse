namespace AuthenticationApi.Infrastructure.Models
{
    public class ProviderLoginModel
    {
        public required string Email { get; set; }
        public required string ProviderLogin { get; set; }
        public required string ProviderKey { get; set; }
    }
}

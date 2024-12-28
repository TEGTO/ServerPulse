namespace AuthenticationApi.Dtos
{
    public class EmailConfirmationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}

namespace AuthenticationApi.Infrastructure
{
    public record class RegisterUserModel
    {
        public required User User { get; set; }
        public required string Password { get; set; }
    }
}

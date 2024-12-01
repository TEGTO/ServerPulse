namespace AuthenticationApi.Dtos
{
    public class CheckAuthDataRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

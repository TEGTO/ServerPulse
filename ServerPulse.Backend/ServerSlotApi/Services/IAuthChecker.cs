namespace ServerSlotApi.Services
{
    public interface IAuthChecker
    {
        public Task<bool> CheckAuthDataAsync(string email, string password, CancellationToken cancellationToken);
    }
}

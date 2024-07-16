namespace ServerSlotApi.Services
{
    public interface IAuthChecker
    {
        public Task<bool> CheckAuthDataAsync(string login, string password, CancellationToken cancellationToken);
    }
}

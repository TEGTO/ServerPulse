
namespace AuthenticationApi.Application.Services
{
    public interface IStringVerifierService
    {
        public Task<string> GetStringVerifierAsync(CancellationToken cancellationToken);
    }
}
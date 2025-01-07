using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;

namespace AuthenticationApi.Services
{
    public interface IEmailJobService
    {
        public Task SendEmailConfirmationMessageAsync(RegisterRequest request);
    }
}
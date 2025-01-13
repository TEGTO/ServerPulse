using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;

namespace AuthenticationApi.Application.Services
{
    public interface IEmailJobService
    {
        public Task SendEmailConfirmationMessageAsync(RegisterRequest request);
    }
}
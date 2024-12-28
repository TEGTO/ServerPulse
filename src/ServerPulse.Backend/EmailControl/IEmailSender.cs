
namespace EmailControl
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken);
    }
}
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    public class EmailClientWrapper : IEmailClientWrapper
    {
        private readonly EmailSettings emailSettings;
        private EmailClient? emailClient;

        public EmailClientWrapper(IOptions<EmailSettings> options)
        {
            emailSettings = options.Value;
        }

        public Task SendAsync(WaitUntil waitUntil, EmailMessage emailMessage, CancellationToken cancellationToken)
        {
            if (emailClient == null)
            {
                emailClient = new EmailClient(emailSettings.ConnectionString);
            }

            return emailClient.SendAsync(waitUntil, emailMessage, cancellationToken);
        }
    }
}

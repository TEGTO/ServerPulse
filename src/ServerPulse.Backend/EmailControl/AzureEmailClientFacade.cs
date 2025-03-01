using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    internal class AzureEmailClientFacade : IAzureEmailClientFacade
    {
        private readonly AzureEmailSettings emailSettings;
        private EmailClient? emailClient;

        public AzureEmailClientFacade(IOptions<AzureEmailSettings> options)
        {
            emailSettings = options.Value;
        }

        public Task<EmailSendOperation> SendAsync(WaitUntil wait, EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (emailClient == null)
            {
                emailClient = new EmailClient(emailSettings.ConnectionString);
            }

            return emailClient.SendAsync(WaitUntil.Completed, message, cancellationToken);
        }
    }
}
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    internal sealed class AzureEmailSender : IEmailSender
    {
        private readonly AzureEmailSettings emailSettings;
        private readonly IAzureEmailClientFacade azureEmailClient;

        public AzureEmailSender(IOptions<AzureEmailSettings> options, IAzureEmailClientFacade azureEmailClient)
        {
            emailSettings = options.Value;
            this.azureEmailClient = azureEmailClient;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            var emailMessage = new EmailMessage(
                 senderAddress: emailSettings.SenderAddress,
                 content: new EmailContent(subject)
                 {
                     PlainText = Helpers.HtmlToPlainText(body),
                     Html = body
                 },
                 recipients: new EmailRecipients([new EmailAddress(toEmail)]));

            await azureEmailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken);
        }
    }
}
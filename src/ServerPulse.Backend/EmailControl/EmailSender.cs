using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings emailSettings;
        private readonly IEmailClientWrapper emailClient;

        public EmailSender(IOptions<EmailSettings> options, IEmailClientWrapper emailClient)
        {
            emailSettings = options.Value;
            this.emailClient = emailClient;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            var emailMessage = new EmailMessage(
                senderAddress: emailSettings.SenderAddress,
                content: new EmailContent(subject)
                {
                    PlainText = HtmlToPlainText(body),
                    Html = body
                },
                recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(toEmail) }));

            await emailClient.SendAsync(WaitUntil.Completed, emailMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static string HtmlToPlainText(string html)
        {
            // Simple conversion of HTML to plain text for text email fallback
            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        }
    }
}

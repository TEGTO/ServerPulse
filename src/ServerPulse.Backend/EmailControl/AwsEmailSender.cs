using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    internal sealed class AwsEmailSender : IEmailSender
    {
        private readonly AwsEmailSettings emailSettings;
        private readonly IAwsEmailClientFacade awsEmailClient;

        public AwsEmailSender(IOptions<AwsEmailSettings> options, IAwsEmailClientFacade awsEmailClient)
        {
            emailSettings = options.Value;
            this.awsEmailClient = awsEmailClient;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = emailSettings.SenderAddress,
                Destination = new Destination
                {
                    ToAddresses = [toEmail]
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(body),
                        Text = new Content(Helpers.HtmlToPlainText(body))
                    }
                }
            };

            await awsEmailClient.SendEmailAsync(sendRequest, cancellationToken);
        }
    }
}
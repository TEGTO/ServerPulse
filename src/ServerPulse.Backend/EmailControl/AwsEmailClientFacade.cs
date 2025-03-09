using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;

namespace EmailControl
{
    internal class AwsEmailClientFacade : IAwsEmailClientFacade
    {
        private readonly AmazonSimpleEmailServiceClient sesClient;

        public AwsEmailClientFacade(IOptions<AwsEmailSettings> options)
        {
            var awsSettings = options.Value;

            var credentials = new BasicAWSCredentials(awsSettings.AccessKey, awsSettings.SecretKey);
            sesClient = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(awsSettings.Region));
        }

        public Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
        {
            return sesClient.SendEmailAsync(request, cancellationToken);
        }
    }
}
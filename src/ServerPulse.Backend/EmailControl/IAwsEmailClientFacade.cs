using Amazon.SimpleEmail.Model;

namespace EmailControl
{
    internal interface IAwsEmailClientFacade
    {
        internal Task<SendEmailResponse> SendEmailAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
    }
}
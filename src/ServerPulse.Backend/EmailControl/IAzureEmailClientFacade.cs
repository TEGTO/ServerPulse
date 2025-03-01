using Azure;
using Azure.Communication.Email;

namespace EmailControl
{
    internal interface IAzureEmailClientFacade
    {
        public Task<EmailSendOperation> SendAsync(WaitUntil wait, EmailMessage message, CancellationToken cancellationToken = default);
    }
}
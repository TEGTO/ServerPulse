using Azure;
using Azure.Communication.Email;

namespace EmailControl
{
    public interface IEmailClientWrapper
    {
        public Task SendAsync(WaitUntil waitUntil, EmailMessage emailMessage, CancellationToken cancellationToken);
    }
}
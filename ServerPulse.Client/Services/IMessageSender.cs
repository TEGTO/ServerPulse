namespace ServerPulse.Client.Services
{
    public interface IMessageSender
    {
        public Task SendJsonAsync(string json, string url, CancellationToken cancellationToken);
    }
}
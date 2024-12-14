using EventCommunication;

namespace ServerMonitorApi.Services
{
    public interface IStatisticsEventSender
    {
        public Task SendLoadEventForStatistics(LoadEvent ev, CancellationToken cancellationToken);
    }
}
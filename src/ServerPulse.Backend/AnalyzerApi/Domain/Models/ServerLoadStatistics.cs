using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Domain.Models
{
    public class ServerLoadStatistics
    {
        public int AmountOfEvents { get; set; }
        public LoadEvent? LastEvent { get; set; }
        public DateTime CollectedDateUTC { get; set; }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Models
{
    public class ServerLoadStatistics
    {
        public int AmountOfEvents { get; set; }
        public LoadEventWrapper? LastEvent { get; set; }
        public LoadMethodStatistics? LoadMethodStatistics { get; set; }
        public DateTime CollectedDateUTC { get; set; }
        public bool IsInitial { get; set; }
    }
}
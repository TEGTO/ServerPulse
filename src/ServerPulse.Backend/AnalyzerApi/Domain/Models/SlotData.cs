using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Models
{
    public class SlotData : BaseStatistics
    {
        public ServerStatistics? GeneralStatistics { get; set; }
        public ServerLoadStatistics? LoadStatistics { get; set; }
        public ServerCustomStatistics? CustomEventStatistics { get; set; }
        public required IEnumerable<LoadEventWrapper> LastLoadEvents { get; set; }
        public required IEnumerable<CustomEventWrapper> LastCustomEvents { get; set; }
    }
}
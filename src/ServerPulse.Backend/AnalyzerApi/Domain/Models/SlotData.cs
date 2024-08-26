using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Models
{
    public class SlotData
    {
        public ServerStatistics GeneralStatistics { get; set; }
        public ServerLoadStatistics LoadStatistics { get; set; }
        public CustomEventStatistics CustomEventStatistics { get; set; }
        public IEnumerable<LoadEventWrapper> LastLoadEvents { get; set; }
        public IEnumerable<CustomEventWrapper> LastCustomEvents { get; set; }
    }
}
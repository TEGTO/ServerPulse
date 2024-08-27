using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;

namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class SlotDataResponse
    {
        public DateTime CollectedDateUTC { get; set; }
        public ServerStatistics? GeneralStatistics { get; set; }
        public ServerLoadStatistics? LoadStatistics { get; set; }
        public CustomEventStatistics? CustomEventStatistics { get; set; }
        public required IEnumerable<LoadEventWrapper> LastLoadEvents { get; set; }
        public required IEnumerable<CustomEventWrapper> LastCustomEvents { get; set; }
    }
}
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class SlotStatisticsResponse
    {
        public DateTime CollectedDateUTC { get; set; }
        public ServerStatistics? GeneralStatistics { get; set; }
        public ServerLoadStatistics? LoadStatistics { get; set; }
        public ServerCustomStatistics? CustomEventStatistics { get; set; }
        public IEnumerable<LoadEventWrapper> LastLoadEvents { get; set; } = [];
        public IEnumerable<CustomEventWrapper> LastCustomEvents { get; set; } = [];
    }
}
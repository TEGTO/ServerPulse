using AnalyzerApi.Infrastructure.Dtos.Responses.Events;

namespace AnalyzerApi.Infrastructure.Dtos.Responses.Statistics
{
    public class SlotStatisticsResponse
    {
        public DateTime CollectedDateUTC { get; set; }
        public ServerLifecycleStatisticsResponse? GeneralStatistics { get; set; }
        public ServerLoadStatisticsResponse? LoadStatistics { get; set; }
        public ServerCustomStatisticsResponse? CustomEventStatistics { get; set; }
        public IEnumerable<LoadEventResponse> LastLoadEvents { get; set; } = [];
        public IEnumerable<CustomEventResponse> LastCustomEvents { get; set; } = [];
    }
}
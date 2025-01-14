using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Dtos.Responses.Statistics;

namespace AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetSlotStatistics
{
    public class GetSlotStatisticsResponse
    {
        public DateTime CollectedDateUTC { get; set; }
        public ServerLifecycleStatisticsResponse? GeneralStatistics { get; set; }
        public ServerLoadStatisticsResponse? LoadStatistics { get; set; }
        public ServerCustomStatisticsResponse? CustomEventStatistics { get; set; }
        public IEnumerable<LoadEventResponse> LastLoadEvents { get; set; } = [];
        public IEnumerable<CustomEventResponse> LastCustomEvents { get; set; } = [];
    }
}

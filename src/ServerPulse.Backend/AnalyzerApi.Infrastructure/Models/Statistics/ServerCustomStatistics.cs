using AnalyzerApi.Infrastructure.Models.Wrappers;

namespace AnalyzerApi.Infrastructure.Models.Statistics
{
    public class ServerCustomStatistics : BaseStatistics
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
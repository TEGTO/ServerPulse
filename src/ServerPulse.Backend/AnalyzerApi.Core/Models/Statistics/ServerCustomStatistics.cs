using AnalyzerApi.Core.Models.Wrappers;

namespace AnalyzerApi.Core.Models.Statistics
{
    public class ServerCustomStatistics : BaseStatistics
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
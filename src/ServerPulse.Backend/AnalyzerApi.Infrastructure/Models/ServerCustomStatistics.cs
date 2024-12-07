using AnalyzerApi.Infrastructure.Wrappers;

namespace AnalyzerApi.Infrastructure.Models
{
    public class ServerCustomStatistics : BaseStatistics
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
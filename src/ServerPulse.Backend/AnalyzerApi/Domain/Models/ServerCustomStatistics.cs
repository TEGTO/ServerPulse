using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Models
{
    public class ServerCustomStatistics : BaseStatistics
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
using AnalyzerApi.Domain.Dtos.Wrappers;

namespace AnalyzerApi.Domain.Models
{
    public class CustomEventStatistics : BaseStatistics
    {
        public CustomEventWrapper? LastEvent { get; set; }
    }
}
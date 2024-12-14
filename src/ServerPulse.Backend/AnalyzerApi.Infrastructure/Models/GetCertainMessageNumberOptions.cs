namespace AnalyzerApi.Infrastructure.Models
{
    public record GetCertainMessageNumberOptions(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);
}

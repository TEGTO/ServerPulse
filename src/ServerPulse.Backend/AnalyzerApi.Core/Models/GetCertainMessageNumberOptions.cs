namespace AnalyzerApi.Core.Models
{
    public record GetCertainMessageNumberOptions(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);
}

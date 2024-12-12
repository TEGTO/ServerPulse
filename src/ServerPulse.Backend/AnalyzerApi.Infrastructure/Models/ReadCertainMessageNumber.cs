namespace AnalyzerApi.Infrastructure.Models
{
    public record ReadCertainMessageNumber(string Key, int NumberOfMessages, DateTime StartDate, bool ReadNew);
}

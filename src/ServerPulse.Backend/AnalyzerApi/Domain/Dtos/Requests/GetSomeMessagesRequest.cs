namespace AnalyzerApi.Domain.Dtos.Requests
{
    public class GetSomeMessagesRequest
    {
        public string Key { get; set; }
        public int NumberOfMessages { get; set; }
        public DateTime StartDate { get; set; }
        public bool ReadNew { get; set; }
    }
}
namespace AnalyzerApi.Domain.Dtos.Responses
{
    public class LoadMethodStatisticsResponse : BaseStatisticsResponse
    {
        public int GetAmount { get; set; }
        public int PostAmount { get; set; }
        public int PutAmount { get; set; }
        public int PatchAmount { get; set; }
        public int DeleteAmount { get; set; }
    }
}
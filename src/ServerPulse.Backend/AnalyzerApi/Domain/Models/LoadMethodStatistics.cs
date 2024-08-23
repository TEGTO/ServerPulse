namespace AnalyzerApi.Domain.Models
{
    public class LoadMethodStatistics : BaseStatistics
    {
        public int GetAmount { get; set; }
        public int PostAmount { get; set; }
        public int PutAmount { get; set; }
        public int PatchAmount { get; set; }
        public int DeleteAmount { get; set; }
    }
}
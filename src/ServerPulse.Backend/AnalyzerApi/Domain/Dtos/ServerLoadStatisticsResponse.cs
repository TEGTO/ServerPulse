namespace AnalyzerApi.Domain.Dtos
{
    public class ServerLoadStatisticsResponse
    {
        public int AmountOfEvents { get; set; }
        public ServerLoadResponse? LastEvent { get; set; }
    }
}

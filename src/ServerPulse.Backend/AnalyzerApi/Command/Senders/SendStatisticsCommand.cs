using AnalyzerApi.Infrastructure.Models.Statistics;
using MediatR;

namespace AnalyzerApi.Command.Senders
{
    public record SendStatisticsCommand<TStatistics>(string Key, TStatistics Statistics) : IRequest<Unit> where TStatistics : BaseStatistics;
}

using AnalyzerApi.Core.Models.Statistics;
using MediatR;

namespace AnalyzerApi.Application.Command.Senders
{
    public record SendStatisticsCommand<TStatistics>(string Key, TStatistics Statistics) : IRequest<Unit> where TStatistics : BaseStatistics;
}

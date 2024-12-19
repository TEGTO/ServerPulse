using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetLoadAmountStatisticsInRange
{
    public class GetLoadAmountStatisticsInRangeQueryHandler : IRequestHandler<GetLoadAmountStatisticsInRangeQuery, IEnumerable<LoadAmountStatisticsResponse>>
    {
        private readonly ILoadAmountStatisticsReceiver receiver;
        private readonly IMapper mapper;

        public GetLoadAmountStatisticsInRangeQueryHandler(ILoadAmountStatisticsReceiver loadAmountStatisticsReceiver, IMapper mapper)
        {
            this.receiver = loadAmountStatisticsReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadAmountStatisticsResponse>> Handle(GetLoadAmountStatisticsInRangeQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new GetInRangeOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await receiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

            return statistics.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }
    }
}

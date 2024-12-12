using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Services.Receivers.Statistics;
using AutoMapper;
using MediatR;

namespace AnalyzerApi.Command.Controllers.GetAmountStatisticsInRange
{
    public class GetAmountStatisticsInRangeQueryHandler : IRequestHandler<GetAmountStatisticsInRangeQuery, IEnumerable<LoadAmountStatisticsResponse>>
    {
        private readonly ILoadAmountStatisticsReceiver receiver;
        private readonly IMapper mapper;

        public GetAmountStatisticsInRangeQueryHandler(ILoadAmountStatisticsReceiver loadAmountStatisticsReceiver, IMapper mapper)
        {
            this.receiver = loadAmountStatisticsReceiver;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<LoadAmountStatisticsResponse>> Handle(GetAmountStatisticsInRangeQuery command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var options = new InRangeQuery(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await receiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

            return statistics.Select(mapper.Map<LoadAmountStatisticsResponse>);
        }
    }
}

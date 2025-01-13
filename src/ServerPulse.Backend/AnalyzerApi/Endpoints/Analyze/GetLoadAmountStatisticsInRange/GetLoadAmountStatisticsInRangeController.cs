using AnalyzerApi.Application.Services.Receivers.Statistics;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Endpoints.Analyze.GetLoadAmountStatisticsInRange
{
    [Route("analyze")]
    [ApiController]
    public class GetLoadAmountStatisticsInRangeController : ControllerBase
    {
        private readonly ILoadAmountStatisticsReceiver receiver;
        private readonly IMapper mapper;

        public GetLoadAmountStatisticsInRangeController(ILoadAmountStatisticsReceiver receiver, IMapper mapper)
        {
            this.receiver = receiver;
            this.mapper = mapper;
        }

        [OutputCache(PolicyName = "GetLoadAmountStatisticsInRangePolicy")]
        [Route("amountrange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadAmountStatisticsResponse>>> GetLoadAmountStatisticsInRange(
            GetLoadAmountStatisticsInRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new GetInRangeOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var statistics = await receiver.GetStatisticsInRangeAsync(options, request.TimeSpan, cancellationToken);

            return Ok(statistics.Select(mapper.Map<LoadAmountStatisticsResponse>));
        }
    }
}

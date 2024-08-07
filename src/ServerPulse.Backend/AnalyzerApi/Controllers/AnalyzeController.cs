using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IServerLoadReceiver serverLoadReceiver;

        public AnalyzeController(IMapper mapper, IServerLoadReceiver serverLoadReceiver)
        {
            this.mapper = mapper;
            this.serverLoadReceiver = serverLoadReceiver;
        }
        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<ServerLoadResponse>>> GetLoadEventsInDataRange([FromBody] LoadEventsRangeRequest request, CancellationToken cancellationToken)
        {
            var events = await serverLoadReceiver.ReceiveEventsInRangeAsync(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime(), cancellationToken);
            return Ok(events.Select(mapper.Map<ServerLoadResponse>));
        }
    }
}
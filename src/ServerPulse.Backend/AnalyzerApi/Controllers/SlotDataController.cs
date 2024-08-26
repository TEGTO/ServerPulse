using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnalyzerApi.Controllers
{
    [Route("slotdata")]
    [ApiController]
    public class SlotDataController : ControllerBase
    {
        [Route("/{key}")]
        [HttpGet]
        public async Task<ActionResult<SlotDataResponse>> GetSlotData([FromRoute] string key, CancellationToken cancellationToken)
        {
            if (events == null)
            {
                var options = new InRangeQueryOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
                events = await serverLoadReceiver.ReceiveEventsInRangeAsync(options, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(events.ToList()), cacheExpiryInMinutes);

            return Ok(events);
        }
    }
}
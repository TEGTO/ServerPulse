using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Command.DeleteStatisticsByKey;

namespace ServerMonitorApi.Controllers
{
    [Route("statisticscontrol")]
    [ApiController]
    public class StatisticsControlController : ControllerBase
    {
        private readonly IMediator mediator;

        public StatisticsControlController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteStatisticsByKey(string key, CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteStatisticsByKeyCommand(key), cancellationToken);
            return Ok();
        }
    }
}
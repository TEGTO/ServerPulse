using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Controllers
{
    [Route("statisticscontrol")]
    [ApiController]
    public class StatisticsControlController : ControllerBase
    {
        private readonly IEventSender messageSender;
        private readonly ISlotKeyChecker serverSlotChecker;
        private readonly IStatisticsControlService statisticsControlService;

        public StatisticsControlController(IEventSender messageSender, ISlotKeyChecker serverSlotChecker, IStatisticsControlService statisticsControlService)
        {
            this.messageSender = messageSender;
            this.serverSlotChecker = serverSlotChecker;
            this.statisticsControlService = statisticsControlService;
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> DeleteStatisticsByKey(string key, CancellationToken cancellationToken)
        {
            await statisticsControlService.DeleteStatisticsByKeyAsync(key);
            return Ok();
        }
    }
}
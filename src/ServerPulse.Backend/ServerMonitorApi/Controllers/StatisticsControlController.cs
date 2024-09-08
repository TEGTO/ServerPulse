using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;

namespace ServerMonitorApi.Controllers
{
    [Route("statisticscontrol")]
    [ApiController]
    public class StatisticsControlController : ControllerBase
    {
        private readonly IStatisticsControlService statisticsControlService;

        public StatisticsControlController(IStatisticsControlService statisticsControlService)
        {
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
using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IServerAnalyzer serverAnalyzer;

        public AnalyzeController(IMapper mapper, IServerAnalyzer serverAnalyzer)
        {
            this.mapper = mapper;
            this.serverAnalyzer = serverAnalyzer;
        }

        [HttpGet("{key}")]
        public async Task<ActionResult<ServerStatisticsResponse>> GetCurrentServerStatusByKey(string key, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Invalid key!");
            }
            var analyzedData = await serverAnalyzer.GetServerStatisticsByKeyAsync(key, cancellationToken);
            var response = mapper.Map<ServerStatisticsResponse>(analyzedData);
            return Ok(response);
        }
    }
}
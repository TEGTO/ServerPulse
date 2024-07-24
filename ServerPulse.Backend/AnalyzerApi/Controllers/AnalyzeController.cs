using AnalyzerApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos.Analyzation;

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
        public async Task<ActionResult<AnalyzedDataReponse>> GetAnalyzedDataByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return BadRequest("Invalid key!");
            }
            var analyzedData = await serverAnalyzer.GetAnalyzedDataByKeyAsync(key);
            var response = mapper.Map<AnalyzedDataReponse>(analyzedData);
            return Ok(response);
        }
    }
}
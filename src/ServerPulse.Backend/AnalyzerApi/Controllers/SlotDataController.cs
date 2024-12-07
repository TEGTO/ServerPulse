using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Controllers
{
    [Route("slotdata")]
    [ApiController]
    public class SlotDataController : ControllerBase
    {
        private readonly ISlotDataPicker dataPicker;
        private readonly IMapper mapper;

        public SlotDataController(ISlotDataPicker dataPicker, IMapper mapper, IConfiguration configuration)
        {
            this.dataPicker = dataPicker;
            this.mapper = mapper;
        }

        [OutputCache(PolicyName = "GetSlotDataPolicy")]
        [Route("{key}")]
        [HttpGet]
        public async Task<ActionResult<SlotDataResponse>> GetSlotData(string key, CancellationToken cancellationToken)
        {
            var data = await dataPicker.GetSlotDataAsync(key, cancellationToken);

            var response = mapper.Map<SlotDataResponse>(data);

            return Ok(response);
        }
    }
}
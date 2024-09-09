using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApi.Services;
using System.Text.Json;

namespace AnalyzerApi.Controllers
{
    [Route("slotdata")]
    [ApiController]
    public class SlotDataController : ControllerBase
    {
        private readonly ISlotDataPicker dataPicker;
        private readonly IMapper mapper;
        private readonly ICacheService cacheService;
        private readonly double cacheExpiryInMinutes;
        private readonly string cacheKey;

        public SlotDataController(ISlotDataPicker dataPicker, IMapper mapper, ICacheService cacheService, IConfiguration configuration)
        {
            this.dataPicker = dataPicker;
            this.mapper = mapper;
            this.cacheService = cacheService;
            cacheExpiryInMinutes = double.Parse(configuration[Configuration.CACHE_EXPIRY_IN_MINUTES]!);
            cacheKey = configuration[Configuration.CACHE_KEY]!;
        }

        [Route("{key}")]
        [HttpGet]
        public async Task<ActionResult<SlotDataResponse>> GetData([FromRoute] string key, CancellationToken cancellationToken)
        {
            var cacheKey = $"{this.cacheKey}-{key}-slotdata";
            SlotData? data = await cacheService.GetInCacheAsync<SlotData>(cacheKey);
            if (data == null)
            {
                data = await dataPicker.GetSlotDataAsync(key, cancellationToken);
            }

            await cacheService.SetValueAsync(cacheKey, JsonSerializer.Serialize(data), cacheExpiryInMinutes);

            var response = mapper.Map<SlotDataResponse>(data);

            return Ok(response);
        }
    }
}
﻿using AnalyzerApi.Application.Services.Receivers.Event;
using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Models;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace AnalyzerApi.Endpoints.Analyze.GetLoadEventsInDataRange
{
    [Route("analyze")]
    [ApiController]
    public class GetLoadEventsInDataRangeController : ControllerBase
    {
        private readonly IEventReceiver<LoadEventWrapper> receiver;
        private readonly IMapper mapper;

        public GetLoadEventsInDataRangeController(IEventReceiver<LoadEventWrapper> receiver, IMapper mapper)
        {
            this.receiver = receiver;
            this.mapper = mapper;
        }

        [OutputCache(PolicyName = "GetLoadEventsInDataRangePolicy")]
        [Route("daterange")]
        [HttpPost]
        public async Task<ActionResult<IEnumerable<LoadEventResponse>>> GetLoadEventsInDataRange(GetLoadEventsInDataRangeRequest request, CancellationToken cancellationToken)
        {
            var options = new GetInRangeOptions(request.Key, request.From.ToUniversalTime(), request.To.ToUniversalTime());
            var events = await receiver.GetEventsInRangeAsync(options, cancellationToken);

            return Ok(events.Select(mapper.Map<LoadEventResponse>));
        }
    }
}

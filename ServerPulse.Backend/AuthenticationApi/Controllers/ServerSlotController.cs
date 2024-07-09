using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Dtos;

namespace AuthenticationApi.Controllers
{
    [Route("serverslot")]
    [ApiController]
    public class ServerSlotController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IServerSlotService serverSlotService;

        public ServerSlotController(IMapper mapper, IServerSlotService serverAuthService)
        {
            this.mapper = mapper;
            this.serverSlotService = serverAuthService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServerSlotDto>> CreateServerSlot([FromBody] CreateServerSlotRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            var serverSlot = mapper.Map<ServerSlot>(request);
            var result = await serverSlotService.CreateServerSlotAsync(serverSlot, cancellationToken);
            var response = mapper.Map<ServerSlotDto>(result);
            return Created(string.Empty, response);
        }
        [HttpGet]
        public async Task<ActionResult<ServerSlotDto>> GetServerSlot([FromQuery] string email,
             [FromQuery] string password,
             [FromQuery] string serverSlotId,
             CancellationToken cancellationToken)
        {
            var serverSlot = await serverSlotService.GetServerSlotAsync(email, password, serverSlotId, cancellationToken);
            if (serverSlot == null)
            {
                return NotFound();
            }
            var response = mapper.Map<ServerSlotDto>(serverSlot);
            return Ok(response);
        }
        [Authorize]
        [HttpGet("{email}")]
        public async Task<ActionResult<IEnumerable<ServerSlotDto>>> GetServerSlotsByEmail([FromRoute] string email, CancellationToken cancellationToken)
        {
            var serverSlots = await serverSlotService.GetServerSlotsByEmailAsync(email, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotDto>));
        }
    }
}

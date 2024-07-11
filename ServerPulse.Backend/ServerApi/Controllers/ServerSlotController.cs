using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerApi.Domain.Dtos;
using ServerApi.Domain.Entities;
using ServerApi.Services;
using Shared.Dtos;
using System.Security.Claims;

namespace ServerApi.Controllers
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

        [HttpPost("get")]
        public async Task<ActionResult<ServerSlotDto>> GetServerSlot([FromBody] GetServerSlotRequest request, CancellationToken cancellationToken)
        {
            var serverSlot = await serverSlotService.GetServerSlotAsync(request.Email, request.Password,
                request.ServerSlotId, cancellationToken);
            if (serverSlot == null)
            {
                return NotFound();
            }
            var response = mapper.Map<ServerSlotDto>(serverSlot);
            return Ok(response);
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServerSlotDto>>> GetServerSlotsByEmail(CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var serverSlots = await serverSlotService.GetServerSlotsByEmailAsync(email, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotDto>));
        }
        [Authorize]
        [HttpGet("{str}")]
        public async Task<ActionResult<IEnumerable<ServerSlotDto>>> GerServerSlotsContainingString(string str, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var serverSlots = await serverSlotService.GerServerSlotsContainingStringAsync(email, str, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotDto>));
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
            var email = User.FindFirstValue(ClaimTypes.Email);
            var serverSlot = new ServerSlot()
            {
                UserEmail = email,
                Name = request.ServerSlotName
            };
            var result = await serverSlotService.CreateServerSlotAsync(serverSlot, cancellationToken);
            var response = mapper.Map<ServerSlotDto>(result);
            return Created(string.Empty, response);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServerSlot(string id, CancellationToken cancellationToken)
        {
            await serverSlotService.DeleteServerSlotByIdAsync(id, cancellationToken);
            return Ok();
        }
    }
}
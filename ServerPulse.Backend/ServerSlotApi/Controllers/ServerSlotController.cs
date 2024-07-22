using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Dtos;
using ServerSlotApi.Services;
using Shared.Dtos.ServerSlot;
using System.Security.Claims;

namespace ServerSlotApi.Controllers
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServerSlotResponse>>> GetSlotsByEmail(CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var serverSlots = await serverSlotService.GetSlotsByEmailAsync(email, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotResponse>));
        }
        [Authorize]
        [HttpGet("{str}")]
        public async Task<ActionResult<IEnumerable<ServerSlotResponse>>> GerSlotsContainingString(string str, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var serverSlots = await serverSlotService.GerSlotsContainingStringAsync(email, str, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotResponse>));
        }
        [Route("/check")]
        [HttpPost]
        public async Task<ActionResult<CheckSlotKeyResponse>> CheckSlotKey([FromBody] CheckSlotKeyRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest("Invalid request");
            }
            var result = await serverSlotService.CheckIfKeyValidAsync(request.SlotKey, cancellationToken);
            var response = new CheckSlotKeyResponse()
            {
                SlotKey = request.SlotKey,
                IsExisting = result
            };
            return Ok(response);
        }
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServerSlotResponse>> CreateSlot([FromBody] CreateServerSlotRequest request,
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
                Name = request.Name
            };
            var result = await serverSlotService.CreateSlotAsync(serverSlot, cancellationToken);
            var response = mapper.Map<ServerSlotResponse>(result);
            return Created(string.Empty, response);
        }
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateSlot([FromBody] UpdateServerSlotRequest request, CancellationToken cancellationToken)
        {
            var serverSlot = mapper.Map<ServerSlot>(request);
            await serverSlotService.UpdateSlotAsync(serverSlot, cancellationToken);
            return Ok();
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(string id, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            await serverSlotService.DeleteSlotByIdAsync(email, id, cancellationToken);
            return Ok();
        }
    }
}
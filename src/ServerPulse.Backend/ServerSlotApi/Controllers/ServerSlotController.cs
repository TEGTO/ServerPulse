using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Services;
using System.Security.Claims;

namespace ServerSlotApi.Controllers
{
    [Route("serverslot")]
    [ApiController]
    public class ServerSlotController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IServerSlotService serverSlotService;
        private readonly ISlotStatisticsService slotStatisticsService;

        public ServerSlotController(IMapper mapper, IServerSlotService serverAuthService, ISlotStatisticsService slotStatisticsService)
        {
            this.mapper = mapper;
            this.serverSlotService = serverAuthService;
            this.slotStatisticsService = slotStatisticsService;
        }

        #region Endpoints

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServerSlotResponse>>> GetSlotsByEmail(CancellationToken cancellationToken)
        {
            var email = GetUserEmail();
            var serverSlots = await serverSlotService.GetSlotsByEmailAsync(email, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotResponse>));
        }
        [Authorize]
        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<ServerSlotResponse>> GetSlotById(string id, CancellationToken cancellationToken)
        {
            var param = new SlotParams(GetUserEmail(), id);

            var serverSlot = await serverSlotService.GetSlotByIdAsync(param, cancellationToken);
            if (serverSlot == null)
            {
                return NotFound();
            }

            var response = mapper.Map<ServerSlotResponse>(serverSlot);
            return Ok(response);
        }
        [Authorize]
        [HttpGet("contains/{str}")]
        public async Task<ActionResult<IEnumerable<ServerSlotResponse>>> GetSlotsContainingString(string str, CancellationToken cancellationToken)
        {
            var email = GetUserEmail();
            var serverSlots = await serverSlotService.GetSlotsContainingStringAsync(email, str, cancellationToken);
            return Ok(serverSlots.Select(mapper.Map<ServerSlotResponse>));
        }
        [Route("/check")]
        [HttpPost]
        public async Task<ActionResult<CheckSlotKeyResponse>> CheckSlotKey([FromBody] CheckSlotKeyRequest request,
            CancellationToken cancellationToken)
        {
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
            var email = GetUserEmail();
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
            var param = new SlotParams(GetUserEmail(), request.Id);
            var serverSlot = mapper.Map<ServerSlot>(request);

            await serverSlotService.UpdateSlotAsync(param, serverSlot, cancellationToken);

            return Ok();
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(string id, CancellationToken cancellationToken)
        {
            var param = new SlotParams(GetUserEmail(), id);

            var slot = await serverSlotService.GetSlotByIdAsync(param, cancellationToken);

            if (slot != null)
            {
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                if (await slotStatisticsService.DeleteSlotStatisticsAsync(slot.SlotKey, token, cancellationToken))
                {
                    await serverSlotService.DeleteSlotByIdAsync(param, cancellationToken);
                }
                else
                {
                    return StatusCode(500, "Failed to delete server slot!");
                }
            }

            return Ok();
        }

        #endregion

        #region Private Helpers 

        private string? GetUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }

        #endregion
    }
}
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServerSlotApi.Command.CheckSlotKey;
using ServerSlotApi.Command.CreateSlot;
using ServerSlotApi.Command.DeleteSlot;
using ServerSlotApi.Command.GetSlotById;
using ServerSlotApi.Command.GetSlotsByEmail;
using ServerSlotApi.Command.UpdateSlot;
using ServerSlotApi.Dtos;
using System.Security.Claims;

namespace ServerSlotApi.Controllers
{
    [Route("serverslot")]
    [ApiController]
    public class ServerSlotController : ControllerBase
    {
        private readonly IMediator mediator;

        public ServerSlotController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        #region Endpoints

        [Authorize]
        [HttpGet]
        [OutputCache(PolicyName = "GetSlotsByEmailPolicy")]
        public async Task<ActionResult<IEnumerable<ServerSlotResponse>>> GetSlotsByEmail([FromQuery] string contains = "", CancellationToken cancellationToken = default)
        {
            var email = GetUserEmail();

            var response = await mediator.Send(new GetSlotsByEmailCommand(email, contains), cancellationToken);

            return Ok(response);
        }

        [Authorize]
        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<ServerSlotResponse?>> GetSlotById(string id, CancellationToken cancellationToken)
        {
            var email = GetUserEmail();

            var response = await mediator.Send(new GetSlotByIdCommand(email, id), cancellationToken);

            if (response == null)
            {
                return NotFound();
            }

            return Ok(response);
        }

        [Route("/check")]
        [HttpPost]
        [OutputCache(PolicyName = "CheckSlotKeyPolicy")]
        public async Task<ActionResult<CheckSlotKeyResponse>> CheckSlotKey(CheckSlotKeyRequest request, CancellationToken cancellationToken)
        {
            var response = await mediator.Send(new CheckSlotKeyCommand(request), cancellationToken);

            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServerSlotResponse>> CreateSlot(CreateServerSlotRequest request, CancellationToken cancellationToken)
        {
            var email = GetUserEmail();

            var response = await mediator.Send(new CreateSlotCommand(email, request), cancellationToken);

            return Created(string.Empty, response);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateSlot([FromBody] UpdateServerSlotRequest request, CancellationToken cancellationToken)
        {
            var email = GetUserEmail();

            await mediator.Send(new UpdateSlotCommand(email, request), cancellationToken);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(string id, CancellationToken cancellationToken)
        {
            var email = GetUserEmail();

            string token = Request?.Headers.Authorization.ToString().Replace("Bearer ", string.Empty) ?? "";

            await mediator.Send(new DeleteSlotCommand(email, id, token), cancellationToken);

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
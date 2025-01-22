using ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.DeleteSlot
{
    [Route("serverslot")]
    [ApiController]
    public class DeleteSlotController : ControllerBase
    {
        private readonly IServerSlotRepository repository;

        public DeleteSlotController(IServerSlotRepository repository)
        {
            this.repository = repository;
        }

        [Authorize]
        [HttpDelete("{id}")]
        [SwaggerOperation(
            Summary = "Delete server slot.",
            Description = "Deletes a server slot for the user by id."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSlot(string id, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var model = new GetSlot() { SlotId = id, UserEmail = email };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot != null)
            {
                await repository.DeleteSlotAsync(slot, cancellationToken);
            }

            return Ok();
        }
    }
}

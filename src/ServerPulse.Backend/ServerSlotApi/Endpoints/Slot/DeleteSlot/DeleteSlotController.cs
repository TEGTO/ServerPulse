using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
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
        public async Task<IActionResult> DeleteSlot(string id, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var model = new SlotModel() { SlotId = id, UserEmail = email };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot != null)
            {
                await repository.DeleteSlotAsync(slot, cancellationToken);
            }

            return Ok();
        }
    }
}

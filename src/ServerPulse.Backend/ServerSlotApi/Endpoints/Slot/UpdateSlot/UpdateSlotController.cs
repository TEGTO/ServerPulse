using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.UpdateSlot;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.UpdateSlot
{
    [Route("serverslot")]
    [ApiController]
    public class UpdateSlotController : ControllerBase
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public UpdateSlotController(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        [Authorize]
        [HttpPut]
        [SwaggerOperation(
            Summary = "Update server slot.",
            Description = "Updates a server slot for the user."
        )]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSlot([FromBody] UpdateSlotRequest request, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var model = new GetSlot() { SlotId = request.Id, UserEmail = email };
            var slotInDb = await repository.GetSlotAsync(model, cancellationToken);

            if (slotInDb == null)
            {
                return Conflict("The slot you are trying to update does not exist!");
            }

            var slot = mapper.Map<ServerSlot>(request);
            slotInDb.Copy(slot);

            await repository.UpdateSlotAsync(slotInDb, cancellationToken);

            return Ok();
        }
    }
}

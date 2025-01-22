using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Core.Models;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.GetSlotById
{
    [Route("serverslot")]
    [ApiController]
    public class GetSlotByIdController : ControllerBase
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public GetSlotByIdController(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        [Authorize]
        [Route("{id}")]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get server slot by id.",
            Description = "Get the server slot for the user by id."
        )]
        [ProducesResponseType(typeof(GetSlotByIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetSlotByIdResponse?>> GetSlotById(string id, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var model = new GetSlot() { SlotId = id, UserEmail = email };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<GetSlotByIdResponse>(slot));
        }
    }
}

using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;
using ServerSlotApi.Core.Entities;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.CreateSlot
{
    [Route("serverslot")]
    [ApiController]
    public class CreateSlotController : ControllerBase
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public CreateSlotController(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        [Authorize]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create server slot.",
            Description = "Creates a server slot for the user."
        )]
        [ProducesResponseType(typeof(CreateSlotResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateSlotResponse>> CreateSlot(CreateSlotRequest request, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var slot = mapper.Map<ServerSlot>(request);
            slot.UserEmail = email;

            var response = mapper.Map<CreateSlotResponse>(await repository.CreateSlotAsync(slot, cancellationToken));

            var locationUri = Url.Action(nameof(GetSlotById), "GetSlotById", new { id = response.Id }, Request.Scheme);

            return Created(locationUri, response);
        }
    }
}

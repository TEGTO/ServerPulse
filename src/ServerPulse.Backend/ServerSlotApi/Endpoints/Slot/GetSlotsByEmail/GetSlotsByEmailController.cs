using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.GetSlotsByEmail;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ServerSlotApi.Endpoints.Slot.GetSlotsByEmail
{
    [Route("serverslot")]
    [ApiController]
    public class GetSlotsByEmailController : ControllerBase
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public GetSlotsByEmailController(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        [Authorize]
        [HttpGet]
        [OutputCache(PolicyName = "GetSlotsByEmailPolicy")]
        [SwaggerOperation(
            Summary = "Get server slots by user email.",
            Description = "Gets server slots for the user by his email using JWT."
        )]
        [ProducesResponseType(typeof(IEnumerable<GetSlotsByEmailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<GetSlotsByEmailResponse>>> GetSlotsByEmail([FromQuery] string contains = "", CancellationToken cancellationToken = default)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var slots = await repository.GetSlotsByUserEmailAsync(email, contains, cancellationToken);

            return Ok(slots.Select(mapper.Map<GetSlotsByEmailResponse>));
        }
    }
}

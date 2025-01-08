using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.GetSlotsByEmail;
using ServerSlotApi.Infrastructure.Repositories;
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

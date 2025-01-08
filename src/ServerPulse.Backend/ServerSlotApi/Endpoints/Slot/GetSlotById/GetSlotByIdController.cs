using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerSlotApi.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
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
        public async Task<ActionResult<GetSlotByIdResponse?>> GetSlotById(string id, CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
            {
                return Conflict("No user email found!");
            }

            var model = new SlotModel() { SlotId = id, UserEmail = email };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<GetSlotByIdResponse>(slot));
        }
    }
}

using ExceptionHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;
using ServerSlotApi.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerSlotApi.Endpoints.Slot.CheckSlotKey
{
    [Route("serverslot")]
    [ApiController]
    public class CheckSlotKeyController : ControllerBase
    {
        private readonly IServerSlotRepository repository;

        public CheckSlotKeyController(IServerSlotRepository repository)
        {
            this.repository = repository;
        }

        [Route("check")]
        [HttpPost]
        [OutputCache(PolicyName = "CheckSlotKeyPolicy")]
        [SwaggerOperation(
            Summary = "Check slot key.",
            Description = "Validates and checks the slot key."
        )]
        [ProducesResponseType(typeof(CheckSlotKeyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseError), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CheckSlotKeyResponse>> CheckSlotKey(CheckSlotKeyRequest request, CancellationToken cancellationToken)
        {
            var key = request.SlotKey;

            var slot = await repository.GetSlotByKeyAsync(key, cancellationToken);

            return Ok(new CheckSlotKeyResponse() { IsExisting = slot != null, SlotKey = key });
        }
    }
}

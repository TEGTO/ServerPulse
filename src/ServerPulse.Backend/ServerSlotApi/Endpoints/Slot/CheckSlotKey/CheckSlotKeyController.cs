using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using ServerSlotApi.Dtos.Endpoints.Slot.CheckSlotKey;
using ServerSlotApi.Infrastructure.Repositories;

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
        public async Task<ActionResult<CheckSlotKeyResponse>> CheckSlotKey(CheckSlotKeyRequest request, CancellationToken cancellationToken)
        {
            var key = request.SlotKey;

            var slot = await repository.GetSlotByKeyAsync(key, cancellationToken);

            return Ok(new CheckSlotKeyResponse() { IsExisting = slot != null, SlotKey = key });
        }
    }
}

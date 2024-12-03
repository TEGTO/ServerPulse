using MediatR;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.CheckSlotKey
{
    public class CheckSlotKeyCommandHandler : IRequestHandler<CheckSlotKeyCommand, CheckSlotKeyResponse>
    {
        private readonly IServerSlotRepository repository;

        public CheckSlotKeyCommandHandler(IServerSlotRepository repository)
        {
            this.repository = repository;
        }

        public async Task<CheckSlotKeyResponse> Handle(CheckSlotKeyCommand command, CancellationToken cancellationToken)
        {
            var key = command.Request.SlotKey;

            var slot = await repository.GetSlotByKeyAsync(key, cancellationToken);

            return new CheckSlotKeyResponse() { IsExisting = slot != null, SlotKey = key };
        }
    }
}

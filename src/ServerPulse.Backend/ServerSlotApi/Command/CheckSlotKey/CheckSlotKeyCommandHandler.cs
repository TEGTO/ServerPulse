using MediatR;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.CheckSlotKey
{
    public class CheckSlotKeyCommandHandler : IRequestHandler<CheckSlotKeyCommand, bool>
    {
        private readonly IServerSlotRepository repository;

        public CheckSlotKeyCommandHandler(IServerSlotRepository repository)
        {
            this.repository = repository;
        }

        public async Task<bool> Handle(CheckSlotKeyCommand command, CancellationToken cancellationToken)
        {
            var slot = await repository.GetSlotByKeyAsync(command.Request.SlotKey, cancellationToken);
            return slot != null;
        }
    }
}

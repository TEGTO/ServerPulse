using MediatR;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.DeleteSlot
{
    public class DeleteSlotCommandHandler : IRequestHandler<DeleteSlotCommand, Unit>
    {
        private readonly IServerSlotRepository repository;

        public DeleteSlotCommandHandler(IServerSlotRepository repository)
        {
            this.repository = repository;
        }

        public async Task<Unit> Handle(DeleteSlotCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(command.Email);

            var model = new SlotModel() { SlotId = command.Id, UserEmail = command.Email! };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot != null)
            {
                await repository.DeleteSlotAsync(slot, cancellationToken);
            }

            return Unit.Value;
        }
    }
}

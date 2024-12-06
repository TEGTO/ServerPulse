using MediatR;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
using ServerSlotApi.Services;

namespace ServerSlotApi.Command.DeleteSlot
{
    public class DeleteSlotCommandHandler : IRequestHandler<DeleteSlotCommand, Unit>
    {
        private readonly IServerSlotRepository repository;
        private readonly ISlotStatisticsService slotStatisticsService;

        public DeleteSlotCommandHandler(IServerSlotRepository repository, ISlotStatisticsService deleteSlotStatisticsService)
        {
            this.repository = repository;
            this.slotStatisticsService = deleteSlotStatisticsService;
        }

        public async Task<Unit> Handle(DeleteSlotCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(command.Email);

            var model = new SlotModel() { SlotId = command.Id, UserEmail = command.Email! };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot != null)
            {
                await repository.DeleteSlotAsync(slot, cancellationToken);
                await slotStatisticsService.DeleteSlotStatisticsAsync(slot.SlotKey, command.Token, cancellationToken);
            }

            return Unit.Value;
        }
    }
}

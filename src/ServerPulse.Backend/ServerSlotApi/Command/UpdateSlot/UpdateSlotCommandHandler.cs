using AutoMapper;
using MediatR;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.UpdateSlot
{
    public class UpdateSlotCommandHandler : IRequestHandler<UpdateSlotCommand, Unit>
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public UpdateSlotCommandHandler(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        public async Task<Unit> Handle(UpdateSlotCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(command.Email);

            var model = new SlotModel() { SlotId = command.Request.Id, UserEmail = command.Email };
            var slotInDb = await repository.GetSlotAsync(model, cancellationToken);

            if (slotInDb == null)
            {
                throw new InvalidOperationException("The slot you are trying to update does not exist!");
            }

            var slot = mapper.Map<ServerSlot>(command.Request);
            slotInDb.Copy(slot);

            await repository.UpdateSlotAsync(slotInDb, cancellationToken);

            return Unit.Value;
        }
    }
}

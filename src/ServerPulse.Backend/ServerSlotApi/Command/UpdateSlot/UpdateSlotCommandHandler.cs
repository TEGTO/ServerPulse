using AutoMapper;
using MediatR;
using ServerSlotApi.Infrastructure.Entities;
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

            var slot = mapper.Map<ServerSlot>(command.Request);
            slot.UserEmail = command.Email!;

            await repository.UpdateSlotAsync(slot, cancellationToken);

            return Unit.Value;
        }
    }
}

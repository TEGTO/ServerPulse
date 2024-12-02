using AutoMapper;
using MediatR;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.CreateSlot
{
    public class CreateSlotCommandHandler : IRequestHandler<CreateSlotCommand, ServerSlotResponse>
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public CreateSlotCommandHandler(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        public async Task<ServerSlotResponse> Handle(CreateSlotCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(command.Email));

            var slot = mapper.Map<ServerSlot>(command.Request);
            slot.UserEmail = command.Email!;

            return mapper.Map<ServerSlotResponse>(await repository.CreateSlotAsync(slot, cancellationToken));
        }
    }
}

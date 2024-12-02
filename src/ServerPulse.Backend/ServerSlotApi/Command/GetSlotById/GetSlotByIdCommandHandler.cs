using AutoMapper;
using MediatR;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.GetSlotById
{
    public class GetSlotByIdCommandHandler : IRequestHandler<GetSlotByIdCommand, ServerSlotResponse>
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public GetSlotByIdCommandHandler(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        public async Task<ServerSlotResponse> Handle(GetSlotByIdCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(command.Email));

            var model = new SlotModel() { SlotId = command.Id, UserEmail = command.Email! };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            return mapper.Map<ServerSlotResponse>(slot);
        }
    }
}

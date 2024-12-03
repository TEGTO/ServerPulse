using AutoMapper;
using MediatR;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Repositories;

namespace ServerSlotApi.Command.GetSlotsByEmail
{
    public class GetSlotsByEmailCommandHandler : IRequestHandler<GetSlotsByEmailCommand, IEnumerable<ServerSlotResponse>>
    {
        private readonly IServerSlotRepository repository;
        private readonly IMapper mapper;

        public GetSlotsByEmailCommandHandler(IServerSlotRepository repository, IMapper mapper)
        {
            this.mapper = mapper;
            this.repository = repository;
        }

        public async Task<IEnumerable<ServerSlotResponse>> Handle(GetSlotsByEmailCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(command.Email);

            var slots = await repository.GetSlotsByUserEmailAsync(command.Email!, command.ContainsString, cancellationToken);

            return slots.Select(mapper.Map<ServerSlotResponse>);
        }
    }
}

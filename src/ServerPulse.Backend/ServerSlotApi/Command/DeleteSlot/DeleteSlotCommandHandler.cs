using MediatR;
using ServerSlotApi.Infrastructure.Models;
using ServerSlotApi.Infrastructure.Repositories;
using Shared.Helpers;

namespace ServerSlotApi.Command.DeleteSlot
{
    public class DeleteSlotCommandHandler : IRequestHandler<DeleteSlotCommand, Unit>
    {
        private readonly IServerSlotRepository repository;
        private readonly IHttpHelper httpHelper;
        private readonly string deleteStatisticsUrl;

        public DeleteSlotCommandHandler(IServerSlotRepository repository, IHttpHelper httpHelper, IConfiguration configuration)
        {
            this.repository = repository;
            this.httpHelper = httpHelper;
            deleteStatisticsUrl = $"{configuration[Configuration.API_GATEWAY]}{configuration[Configuration.STATISTICS_DELETE_URL]}";
        }

        public async Task<Unit> Handle(DeleteSlotCommand command, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(command.Email));

            var model = new SlotModel() { SlotId = command.Id, UserEmail = command.Email! };

            var slot = await repository.GetSlotAsync(model, cancellationToken);

            if (slot != null)
            {
                await repository.DeleteSlotAsync(slot, cancellationToken);
                await DeleteSlotStatisticsAsync(slot.SlotKey, command.Token, cancellationToken);
            }

            return Unit.Value;
        }

        private async Task DeleteSlotStatisticsAsync(string key, string token, CancellationToken cancellationToken)
        {
            var requestUrl = deleteStatisticsUrl + key;
            await httpHelper.SendDeleteRequestAsync(requestUrl, token, cancellationToken);
        }
    }
}

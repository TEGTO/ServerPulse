﻿using Refit;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;

namespace ServerMonitorApi.Infrastructure.Services
{
    public interface IServerSlotApi
    {
        [Post(ExternalEndpoints.SERVER_SLOT_CHECK)]
        public Task<CheckSlotKeyResponse> CheckSlotKeyAsync(CheckSlotKeyRequest request, CancellationToken cancellationToken);
    }
}

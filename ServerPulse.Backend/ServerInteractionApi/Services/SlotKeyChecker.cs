﻿using Shared.Dtos.ServerSlot;
using System.Text;
using System.Text.Json;

namespace ServerInteractionApi.Services
{
    public class SlotKeyChecker : ISlotKeyChecker
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IRedisService redisService;
        private readonly IConfiguration configuration;
        private readonly string serverSlotApi;
        private readonly int partitionsAmount;
        private readonly double redisExpiryInMinutes;

        public SlotKeyChecker(IHttpClientFactory httpClientFactory, IRedisService redisService, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.redisService = redisService;
            this.configuration = configuration;
            serverSlotApi = configuration[Configuration.SERVER_SLOT_API]!;
            redisExpiryInMinutes = double.Parse(configuration[Configuration.REDIS_SERVER_SLOT_EXPIRY_IN_MINUTES]!);
        }

        public async Task<bool> CheckSlotKeyAsync(string slotKey, CancellationToken cancellationToken)
        {
            if (await CheckSlotKeyInRedisAsync(slotKey))
            {
                return true;
            }

            var httpClient = httpClientFactory.CreateClient();
            var checkServerSlotRequest = new CheckSlotKeyRequest()
            {
                SlotKey = slotKey,
            };
            var jsonContent = new StringContent
            (
                JsonSerializer.Serialize(checkServerSlotRequest),
                Encoding.UTF8,
                "application/json"
            );
            var checkUrl = serverSlotApi + "/check";

            var httpResponseMessage = await httpClient.PostAsync(checkUrl, jsonContent, cancellationToken);
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<CheckSlotKeyResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response != null && response.IsExisting)
            {
                await redisService.SetValueAsync(slotKey, JsonSerializer.Serialize(response), redisExpiryInMinutes);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckSlotKeyInRedisAsync(string slotKey)
        {
            var json = await redisService.GetValueAsync(slotKey);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }
            var response = JsonSerializer.Deserialize<CheckSlotKeyResponse>(json);
            return response.IsExisting;
        }
    }
}
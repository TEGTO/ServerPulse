﻿namespace ServerSlotApi.Dtos.Endpoints.ServerSlot.GetSlotsByEmail
{
    public class GetSlotsByEmailResponse
    {
        public string? Id { get; set; }
        public string? UserEmail { get; set; }
        public string? Name { get; set; }
        public string? SlotKey { get; set; }
    }
}

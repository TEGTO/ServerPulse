﻿namespace AnalyzerApi.Infrastructure.Requests
{
    public class GetSomeMessagesRequest
    {
        public string Key { get; set; } = string.Empty;
        public int NumberOfMessages { get; set; }
        public DateTime StartDate { get; set; }
        public bool ReadNew { get; set; }
    }
}
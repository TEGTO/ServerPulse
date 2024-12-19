namespace Resilience
{
    public class ResiliencePipelineConfiguration
    {
        public int MaxRetryCount { get; set; } = 2;
        public int RetryWaitInSeconds { get; set; } = 2;
        public int CircuitWaitInSeconds { get; set; } = 30;
        public float CircuitFailureRatio { get; set; } = 0.5f;
        public int CircuitMinimumThroughput { get; set; } = 10;
    }
}

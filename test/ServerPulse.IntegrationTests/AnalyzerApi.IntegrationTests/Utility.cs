namespace AnalyzerApi.IntegrationTests
{
    internal static class Utility
    {
        public static async Task WaitUntil(Func<bool> condition, TimeSpan timeout, TimeSpan pollInterval)
        {
            var startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < timeout)
            {
                if (condition())
                    return;

                await Task.Delay(pollInterval);
            }

            Assert.Fail("Timeout exceeded before condition was met.");
        }
    }
}

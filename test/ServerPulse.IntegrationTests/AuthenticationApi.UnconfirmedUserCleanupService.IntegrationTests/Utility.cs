namespace AuthenticationApi.UnconfirmedUserCleanupService.IntegrationTests
{
    internal static class Utility
    {
        public static async Task WaitUntil(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan pollInterval)
        {
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                if (await condition())
                    return;

                await Task.Delay(pollInterval);
            }

            Assert.Fail("Timeout exceeded before condition was met.");
        }
    }
}

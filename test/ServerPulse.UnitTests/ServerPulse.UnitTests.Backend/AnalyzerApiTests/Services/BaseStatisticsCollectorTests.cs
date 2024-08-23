using AnalyzerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApiTests.Services
{
    internal abstract class BaseStatisticsCollectorTests
    {
        protected Mock<IStatisticsSender> mockStatisticsSender;
        protected Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public virtual void Setup()
        {
            mockStatisticsSender = new Mock<IStatisticsSender>();
            mockConfiguration = new Mock<IConfiguration>();
        }

        protected async IAsyncEnumerable<T> AsyncEnumerable<T>(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}
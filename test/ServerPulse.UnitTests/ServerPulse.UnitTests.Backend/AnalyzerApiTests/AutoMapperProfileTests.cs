using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Models;
using AuthenticationApi;
using AutoMapper;

namespace AnalyzerApiTests
{
    [TestFixture]
    internal class AutoMapperProfileTests
    {
        private IMapper mapper;

        [SetUp]
        public void SetUp()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
            mapper = config.CreateMapper();
        }

        [Test]
        public void ServerStatusToAnalyzedDataReponse_MappedCorrectly()
        {
            // Arrange
            var serverStatus = new ServerStatistics
            {
                IsAlive = true
            };
            // Act
            var result = mapper.Map<ServerStatisticsResponse>(serverStatus);
            // Assert
            Assert.That(result.IsAlive, Is.EqualTo(serverStatus.IsAlive));
        }
    }
}
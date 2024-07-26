using AnalyzerApi.Domain.Dtos;
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
            var serverStatus = new ServerStatus
            {
                IsServerAlive = true
            };
            // Act
            var result = mapper.Map<AnalyzedDataReponse>(serverStatus);
            // Assert
            Assert.That(result.IsServerAlive, Is.EqualTo(serverStatus.IsServerAlive));
        }
    }
}
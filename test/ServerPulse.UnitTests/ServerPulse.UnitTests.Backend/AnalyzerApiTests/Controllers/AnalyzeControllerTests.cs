using AnalyzerApi.Controllers;
using AutoMapper;
using Moq;

namespace AnalyzerApiTests.Controllers
{
    [TestFixture]
    internal class AnalyzeControllerTests
    {
        private Mock<IMapper> mockMapper;
        private AnalyzeController controller;

        [SetUp]
        public void SetUp()
        {
            mockMapper = new Mock<IMapper>();
            controller = new AnalyzeController(mockMapper.Object);
        }

    }
}
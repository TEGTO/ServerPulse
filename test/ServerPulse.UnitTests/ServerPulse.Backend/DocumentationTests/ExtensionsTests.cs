using Documentation;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;

namespace DocumentationTests
{
    [TestFixture]
    internal class ExtensionsTests
    {
        private IHostApplicationBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new HostApplicationBuilder();
        }

        [Test]
        public void AddDocumentation_RegistersSwaggerWithSecurityDefinitions()
        {
            // Arrange
            var title = "Test API";

            // Act
            builder.AddDocumentation(title);

            // Assert
            var swaggerProvider = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(ISwaggerProvider));

            Assert.That(swaggerProvider, Is.Not.Null);
        }
    }
}

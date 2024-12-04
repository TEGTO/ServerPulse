using Caching.Services;
using Microsoft.AspNetCore.OutputCaching;
using Moq;
using System.Text.Json;

namespace Caching.Tests
{
    [TestFixture]
    internal class CacheExtensionsTests
    {
        private class TestObject
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private Mock<ICacheService> cacheServiceMock;
        private Mock<OutputCacheOptions> mockOptions;
        private CancellationToken cancellationToken;

        [SetUp]
        public void SetUp()
        {
            cacheServiceMock = new Mock<ICacheService>();
            mockOptions = new Mock<OutputCacheOptions>();
            cancellationToken = CancellationToken.None;
        }

        [TestCase("{\"Id\":1,\"Name\":\"Test\"}", true, 1, "Test")]
        [TestCase("invalid json", false, 0, null)]
        [TestCase(null, false, 0, null)]
        public async Task TryGetAsync_WithVariousInputs_ReturnsExpectedResult(string? cacheValue, bool isValid, int expectedId, string? expectedName)
        {
            // Arrange
            var key = "testKey";

            cacheServiceMock.Setup(s => s.GetAsync(key, cancellationToken)).ReturnsAsync(cacheValue);

            // Act
            var result = await cacheServiceMock.Object.TryGetAsync<TestObject>(key, cancellationToken);

            // Assert
            if (isValid)
            {
                Assert.IsNotNull(result);
                Assert.That(result?.Id, Is.EqualTo(expectedId));
                Assert.That(result?.Name, Is.EqualTo(expectedName));
            }
            else
            {
                Assert.IsNull(result);
            }
        }

        [TestCase(true, true, Description = "Serialization succeeds and SetAsync returns true")]
        [TestCase(false, false, Description = "Serialization fails")]
        public async Task TrySetAsync_WithVariousInputs_ReturnsExpectedResult(bool isSerializationSuccessful, bool expectedResult)
        {
            // Arrange
            var key = "testKey";
            var value = new TestObject { Id = 1, Name = "Test" };

            if (isSerializationSuccessful)
            {
                var serializedValue = JsonSerializer.Serialize(value);
                cacheServiceMock
                    .Setup(s => s.SetAsync(key, serializedValue, It.IsAny<TimeSpan>(), cancellationToken))
                    .ReturnsAsync(true);
            }
            else
            {
                cacheServiceMock
                    .Setup(s => s.SetAsync(key, It.IsAny<string>(), It.IsAny<TimeSpan>(), cancellationToken))
                    .Throws<JsonException>();
            }

            // Act
            var result = await cacheServiceMock.Object.TrySetAsync(key, value, TimeSpan.FromMinutes(5), cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("PolicyWithType", 30, true, typeof(TestObject), Description = "With type and properties")]
        [TestCase("PolicyWithoutType", null, false, null, Description = "Without type")]
        [TestCase("PolicyDefaultDuration", null, true, null, Description = "Without type, useAuthId true")]
        public void SetOutputCachePolicy_WithVariousInputs_DoesNotThrowExceptions(string name, int? durationSeconds, bool useAuthId, Type? type)
        {
            // Arrange
            TimeSpan? duration = durationSeconds.HasValue ? TimeSpan.FromSeconds(durationSeconds.Value) : null;

            // Act + Assert
            Assert.DoesNotThrow(() =>
              CacheExtensions.SetOutputCachePolicy(mockOptions.Object, name, duration, useAuthId, type));
        }

        [Test]
        public void SetOutputCachePolicy_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                CacheExtensions.SetOutputCachePolicy(null!, "Policy", TimeSpan.FromSeconds(30), false));
        }
    }
}
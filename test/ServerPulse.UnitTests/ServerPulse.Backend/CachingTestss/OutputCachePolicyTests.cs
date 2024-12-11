using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;
using Moq;
using System.Security.Claims;
using System.Text;

namespace Caching.Tests
{
    [TestFixture]
    internal class OutputCachePolicyTests
    {
        private Mock<HttpContext> httpContextMock;
        private OutputCacheContext cacheContext;
        private Mock<HttpRequest> requestMock;
        private Mock<HttpResponse> responseMock;
        private ClaimsPrincipal user;

        [SetUp]
        public void SetUp()
        {
            httpContextMock = new Mock<HttpContext>();
            requestMock = new Mock<HttpRequest>();
            responseMock = new Mock<HttpResponse>();
            user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "userId") }));

            httpContextMock.Setup(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.Setup(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.Setup(ctx => ctx.User).Returns(user);

            cacheContext = new OutputCacheContext() { HttpContext = httpContextMock.Object };
        }

        [TestCase(5, ExpectedResult = true)]
        [TestCase(null, ExpectedResult = true)]
        public async Task<bool> CacheRequestAsync_WithDuration_SetsResponseExpirationTimeSpan(int? minutes)
        {
            // Arrange
            var duration = minutes.HasValue ? TimeSpan.FromMinutes(minutes.Value) : (TimeSpan?)null;
            var policy = new OutputCachePolicy(duration, false);

            // Act
            await policy.CacheRequestAsync(cacheContext, CancellationToken.None);

            // Assert
            return cacheContext.ResponseExpirationTimeSpan == duration;
        }

        [TestCase("GET", true)]
        [TestCase("PATCH", false)]
        public async Task CacheRequestAsync_AttemptOutputCaching_VariesByMethod(string method, bool expectedResult)
        {
            // Arrange
            requestMock.Setup(r => r.Method).Returns(method);
            var policy = new OutputCachePolicy();

            // Act
            await policy.CacheRequestAsync(cacheContext, CancellationToken.None);

            // Assert
            Assert.That(cacheContext.AllowCacheLookup, Is.EqualTo(expectedResult));
            Assert.That(cacheContext.AllowCacheStorage, Is.EqualTo(expectedResult));
        }

        [TestCase("userId", true)]
        [TestCase(null, false)]
        public async Task CacheRequestAsync_UseAuthenticationId_AddsUserIdToCacheKeyPrefix(string? userId, bool shouldContainUserId)
        {
            // Arrange
            if (userId != null)
            {
                user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }));
                httpContextMock.Setup(ctx => ctx.User).Returns(user);
            }
            else
            {
                httpContextMock.Setup(ctx => ctx.User).Returns(new ClaimsPrincipal());
            }

            var policy = new OutputCachePolicy(null, true);

            // Act
            await policy.CacheRequestAsync(cacheContext, CancellationToken.None);

            // Assert
            if (shouldContainUserId)
            {
                Assert.That(cacheContext.CacheVaryByRules.CacheKeyPrefix, Is.Not.Null);
                Assert.That(cacheContext.CacheVaryByRules.CacheKeyPrefix.Contains(userId!));
            }
            else
            {
                Assert.That(cacheContext.CacheVaryByRules.CacheKeyPrefix, Is.Null);
            }
        }

        [TestCase("{\"Name\":\"TestName\",\"Id\":\"123\"}", "Name", "TestName")]
        [TestCase("{\"Name\":\"TestName\",\"Id\":\"123\"}", "Id", "123")]
        public async Task CacheRequestAsync_PostRequestWithJsonContentType_SetsVaryByValues(string json, string property, string expectedValue)
        {
            // Arrange
            var policy = new OutputCachePolicy(null, false, property);
            requestMock.Setup(r => r.Method).Returns("POST");
            requestMock.Setup(r => r.ContentType).Returns("application/json");
            requestMock.Setup(r => r.Body).Returns(new MemoryStream(Encoding.UTF8.GetBytes(json)));

            // Act
            await policy.CacheRequestAsync(cacheContext, CancellationToken.None);

            // Assert
            Assert.That(cacheContext.CacheVaryByRules.VaryByValues[property], Is.EqualTo(expectedValue));
        }

        [TestCase("Set-Cookie", "testcookie", false)]
        [TestCase(null, null, true)]
        public async Task CacheRequestAsync_RequestWithOrWithoutCookie_DisablesCacheStorage(string? headerKey, string? headerValue, bool expectedAllowCacheStorage)
        {
            // Arrange
            var policy = new OutputCachePolicy();

            if (headerKey != null)
            {
                responseMock.Setup(r => r.Headers).Returns(new HeaderDictionary { { headerKey, new StringValues(headerValue) } });
            }

            responseMock.Setup(r => r.StatusCode).Returns(StatusCodes.Status200OK);

            cacheContext.AllowCacheStorage = true;

            // Act
            await policy.ServeResponseAsync(cacheContext, CancellationToken.None);

            // Assert
            Assert.That(cacheContext.AllowCacheStorage, Is.EqualTo(expectedAllowCacheStorage));
        }

        [TestCase(StatusCodes.Status200OK, true)]
        [TestCase(StatusCodes.Status404NotFound, false)]
        public async Task CacheRequestAsync_Non200Or301Status_DisablesCacheStorage(int statusCode, bool expectedAllowCacheStorage)
        {
            // Arrange
            var policy = new OutputCachePolicy();
            responseMock.Setup(r => r.StatusCode).Returns(statusCode);

            cacheContext.AllowCacheStorage = true;

            // Act
            await policy.ServeResponseAsync(cacheContext, CancellationToken.None);

            // Assert
            Assert.That(cacheContext.AllowCacheStorage, Is.EqualTo(expectedAllowCacheStorage));
        }
    }
}
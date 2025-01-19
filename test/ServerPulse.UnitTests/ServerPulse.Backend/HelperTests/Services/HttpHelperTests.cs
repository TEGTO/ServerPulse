using Helper.Services;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;

namespace HelperTests.Services.Tests
{
    [TestFixture]
    internal class HttpHelperTests
    {
        private Mock<IHttpClientFactory> httpClientFactoryMock;
        private MockHttpMessageHandler mockHttp;
        private HttpHelper httpHelper;

        [SetUp]
        public void SetUp()
        {
            httpClientFactoryMock = new Mock<IHttpClientFactory>();

            mockHttp = new MockHttpMessageHandler();

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttp.ToHttpClient());

            httpHelper = new HttpHelper(httpClientFactoryMock.Object);
        }
        [TearDown]
        public void TearDown()
        {
            mockHttp.Dispose();
        }

        [Test]
        [TestCase("https://example.com/api/data", null, null, "someheader", HttpStatusCode.OK, "{\"data\":\"test\"}", "test")]
        [TestCase("https://example.com/api/data", "token123", null, "someheader", HttpStatusCode.OK, "{\"data\":\"secure\"}", "secure")]
        [TestCase("https://example.com/api/data", null, "?param=value", "someheader", HttpStatusCode.OK, "{\"data\":\"query\"}", "query")]
        public async Task SendGetRequestAsync_ValidRequest_ReturnsDeserializedData(
            string endpoint,
            string? accessToken,
            string? queryParams,
            string? headers,
            HttpStatusCode statusCode,
            string responseBody,
            string expectedData)
        {
            // Arrange
            var finalUrl = endpoint + (queryParams ?? string.Empty);

            mockHttp.When(HttpMethod.Get, finalUrl).Respond(HttpStatusCode.OK, "application/json", responseBody);

            // Act
            var result = await httpHelper.SendGetRequestAsync<TestResponse>(
                endpoint,
                queryParams: queryParams != null ? new Dictionary<string, string> { { "param", "value" } } : null,
                headers: headers != null ? new Dictionary<string, string> { { "someheader", "value" } } : null,
                accessToken: accessToken);

            // Assert
            Assert.NotNull(result);
            Assert.That(result?.Data, Is.EqualTo(expectedData));
        }

        [Test]
        [TestCase("https://example.com/api/data", "token123", HttpStatusCode.BadRequest, "{\"error\":\"Invalid request\"}")]
        public void SendGetRequestAsync_InvalidRequest_ThrowsHttpRequestException(
            string endpoint,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody)
        {
            // Arrange
            mockHttp.When(HttpMethod.Get, endpoint).Respond(HttpStatusCode.BadRequest, "application/json", responseBody);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.SendGetRequestAsync<TestResponse>(endpoint, null, accessToken: accessToken));

            Assert.That(exception.Message, Is.EqualTo(responseBody));
        }

        [Test]
        [TestCase("https://example.com/api/data", "{\"key\":\"value\"}", "token123", HttpStatusCode.OK, "{\"data\":\"created\"}", "created")]
        [TestCase("https://example.com/api/data", "{\"name\":\"test\"}", null, HttpStatusCode.OK, "{\"data\":\"success\"}", "success")]
        public async Task SendPostRequestAsync_Json_ValidRequest_ReturnsDeserializedData(
            string endpoint,
            string jsonBody,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody,
            string expectedData)
        {
            // Arrange
            mockHttp.When(HttpMethod.Post, endpoint).Respond(statusCode, "application/json", responseBody);

            // Act
            var result = await httpHelper.SendPostRequestAsync<TestResponse>(endpoint, jsonBody, accessToken);

            // Assert
            Assert.NotNull(result);
            Assert.That(result?.Data, Is.EqualTo(expectedData));
        }

        [Test]
        [TestCase("https://example.com/api/data", "{\"key\":\"value\"}", "token123", HttpStatusCode.BadRequest, "{\"error\":\"Invalid request\"}")]
        public void SendPostRequestAsync_Json_InvalidRequest_ThrowsHttpRequestException(
            string endpoint,
            string jsonBody,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody)
        {
            // Arrange
            mockHttp.When(HttpMethod.Post, endpoint).Respond(statusCode, "application/json", responseBody);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.SendPostRequestAsync<TestResponse>(endpoint, jsonBody, accessToken));
            Assert.That(exception.Message, Is.EqualTo(responseBody));
        }

        [Test]
        [TestCase("https://example.com/api/data", "key=value", "token123", HttpStatusCode.OK, "{\"data\":\"form submitted\"}", "form submitted")]
        public async Task SendPostRequestAsync_FormUrlEncoded_ValidRequest_ReturnsDeserializedData(
            string endpoint,
            string formData,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody,
            string expectedData)
        {
            // Arrange
            mockHttp.When(HttpMethod.Post, endpoint).Respond(statusCode, "application/json", responseBody);

            var bodyParams = formData.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(parts => parts[0], parts => parts[1]);

            // Act
            var result = await httpHelper.SendPostRequestAsync<TestResponse>(endpoint, bodyParams, accessToken);

            // Assert
            Assert.NotNull(result);
            Assert.That(result?.Data, Is.EqualTo(expectedData));
        }

        [Test]
        [TestCase("https://example.com/api/data", "key=value", "token123", HttpStatusCode.BadRequest, "{\"error\":\"Invalid request\"}")]
        public void SendPostRequestAsync_FormUrlEncoded_InvalidRequest_ThrowsHttpRequestException(
            string endpoint,
            string formData,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody)
        {
            // Arrange
            mockHttp.When(HttpMethod.Post, endpoint).Respond(statusCode, "application/json", responseBody);

            var bodyParams = formData.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(parts => parts[0], parts => parts[1]);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.SendPostRequestAsync<TestResponse>(endpoint, bodyParams, accessToken));
            Assert.That(exception.Message, Is.EqualTo(responseBody));
        }

        [Test]
        [TestCase("https://example.com/api/data", "{\"key\":\"updated\"}", null, "?param=value", HttpStatusCode.OK)]
        public async Task SendPutRequestAsync_ValidRequest_SuccessfulExecution(
            string endpoint,
            string jsonBody,
            string? accessToken,
            string? queryParams,
            HttpStatusCode statusCode)
        {
            // Arrange
            var finalUrl = endpoint + (queryParams ?? string.Empty);
            mockHttp.When(HttpMethod.Put, finalUrl).Respond(statusCode);

            // Act
            await httpHelper.SendPutRequestAsync(endpoint, jsonBody, queryParams != null ? new Dictionary<string, string> { { "param", "value" } } : null, accessToken);

            // Assert
            Assert.Pass();
        }

        [Test]
        [TestCase("https://example.com/api/data", "{\"key\":\"invalid\"}", null, HttpStatusCode.BadRequest, "{\"error\":\"Invalid data\"}")]
        public void SendPutRequestAsync_InvalidRequest_ThrowsHttpRequestException(
            string endpoint,
            string jsonBody,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody)
        {
            // Arrange
            mockHttp.When(HttpMethod.Put, endpoint).Respond(statusCode, "application/json", responseBody);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.SendPutRequestAsync(endpoint, jsonBody, null, accessToken));
            Assert.That(exception.Message, Is.EqualTo(responseBody));
        }

        [Test]
        [TestCase("https://example.com/api/data", null, HttpStatusCode.OK)]
        [TestCase("https://example.com/api/data", "token123", HttpStatusCode.OK)]
        public async Task SendDeleteRequestAsync_ValidRequest_SuccessfulExecution(
            string endpoint,
            string? accessToken,
            HttpStatusCode statusCode)
        {
            // Arrange
            mockHttp.When(HttpMethod.Get, endpoint).Respond(statusCode);

            // Act
            await httpHelper.SendDeleteRequestAsync(endpoint, accessToken);

            // Assert
            Assert.Pass();
        }

        [Test]
        [TestCase("https://example.com/api/data", null, HttpStatusCode.BadRequest, "{\"error\":\"Invalid request\"}")]
        [TestCase("https://example.com/api/data", "token123", HttpStatusCode.Unauthorized, "{\"error\":\"Unauthorized access\"}")]
        public void SendDeleteRequestAsync_InvalidRequest_ThrowsHttpRequestException(
            string endpoint,
            string? accessToken,
            HttpStatusCode statusCode,
            string responseBody)
        {
            // Arrange
            mockHttp.When(HttpMethod.Get, endpoint).Respond(statusCode, "application/json", responseBody);

            // Act & Assert
            var exception = Assert.ThrowsAsync<HttpRequestException>(async () =>
                await httpHelper.SendDeleteRequestAsync(endpoint, accessToken));
            Assert.That(exception.Message, Is.EqualTo(responseBody));
        }
    }

    public class TestResponse
    {
        public string? Data { get; set; }
    }
}
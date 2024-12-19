using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
using EventCommunication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class GetSomeLoadEventsAnalyzerControllerTests : BaseIntegrationTest
    {
        private string key;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            key = Guid.NewGuid().ToString();

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                new LoadEvent(key, "/api/resource", "GET", 200, TimeSpan.FromMilliseconds(150), DateTime.UtcNow)
            });

            await Task.Delay(1000);

            await SendEventsAsync(LOAD_TOPIC, key, new[]
            {
                 new LoadEvent(key, "/api/resource", "POST", 201, TimeSpan.FromMilliseconds(200), DateTime.UtcNow)
            });
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadLastEvent_ReturnsOkWithLastEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 1, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));

            Assert.True(events[0].Method == "POST");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadTwoLastEvents_ReturnsOkWithLastEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 2, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].Method == "POST");
            Assert.True(events[1].Method == "GET");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadFirstEvent_ReturnsOkWithFirstEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));

            Assert.True(events[0].Method == "GET");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToReadTwoFirstEvents_ReturnsOkWithTwoFirstEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 2, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].Method == "GET");
            Assert.True(events[1].Method == "POST");
        }

        [Test]
        public async Task GetSomeLoadEvents_ValidRequestToMoreEventsThanExist_ReturnsOkWithTwoFirstEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 10, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].Method == "GET");
            Assert.True(events[1].Method == "POST");
        }

        private static IEnumerable<TestCaseData> GetBadRequestTestCases()
        {
            yield return new TestCaseData(
                new GetSomeMessagesRequest { Key = "", NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription("Invalid Key, must be not empty.");

            yield return new TestCaseData(
                new GetSomeMessagesRequest { Key = null!, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription("Invalid Key, must be not null.");

            yield return new TestCaseData(
                new GetSomeMessagesRequest { Key = "someRandomKey281", NumberOfMessages = 0, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription("Invalid NumberOfMessages, must be greater than 0.");

            var maxNumberOfMessages = 20;

            yield return new TestCaseData(
                new GetSomeMessagesRequest { Key = "SomeRandomKey151", NumberOfMessages = maxNumberOfMessages + 1, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription($"Invalid NumberOfMessages, must be less or equal than limit ({maxNumberOfMessages}).");
        }

        [Test]
        [TestCaseSource(nameof(GetBadRequestTestCases))]
        public async Task GetSomeLoadEvents_InvalidRequest_ReturnsBadRequest(GetSomeMessagesRequest request)
        {
            // Arrange
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetSomeLoadEvents_WrongKey_ReturnsEmptyArray()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = Guid.NewGuid().ToString(), NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/someevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<LoadEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }
    }
}

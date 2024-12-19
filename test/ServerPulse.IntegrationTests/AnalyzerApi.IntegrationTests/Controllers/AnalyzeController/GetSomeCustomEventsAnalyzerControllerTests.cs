using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.Requests;
using EventCommunication;
using System.Net;
using System.Text;
using System.Text.Json;

namespace AnalyzerApi.IntegrationTests.Controllers.AnalyzeController
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    internal class GetSomeCustomEventsAnalyzerControllerTests : BaseIntegrationTest
    {
        private string key;

        private readonly List<string> customEvents = new List<string>();

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            key = Guid.NewGuid().ToString();

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(key, "Request1", "Description1")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, key,
            [
                customEvents[0]
            ]);

            await Task.Delay(1000);

            customEvents.Add(JsonSerializer.Serialize(new CustomEvent(key, "Request2", "Description2")));
            await SendCustomEventsAsync(CUSTOM_TOPIC, key,
            [
                 customEvents[1]
            ]);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadLastEvent_ReturnsOkWithLastEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 1, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));

            Assert.True(events[0].SerializedMessage == customEvents[1]);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadTwoLastEvents_ReturnsOkWithLastEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 2, StartDate = DateTime.UtcNow, ReadNew = false };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].SerializedMessage == customEvents[1]);
            Assert.True(events[1].SerializedMessage == customEvents[0]);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadFirstEvent_ReturnsOkWithFirstEvent()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(1));

            Assert.True(events[0].SerializedMessage == customEvents[0]);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToReadTwoFirstEvents_ReturnsOkWithTwoFirstEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 2, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].SerializedMessage == customEvents[0]);
            Assert.True(events[1].SerializedMessage == customEvents[1]);
        }

        [Test]
        public async Task GetSomeCustomEvents_ValidRequestToMoreEventsThanExist_ReturnsOkWithTwoFirstEvents()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = key, NumberOfMessages = 10, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();

            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);

            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));

            Assert.True(events[0].SerializedMessage == customEvents[0]);
            Assert.True(events[1].SerializedMessage == customEvents[1]);
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
                new GetSomeMessagesRequest { Key = "someRandomKey19", NumberOfMessages = 0, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription("Invalid NumberOfMessages, must be greater than 0.");

            var maxNumberOfMessages = 20;

            yield return new TestCaseData(
                new GetSomeMessagesRequest { Key = "someRandomKey129", NumberOfMessages = maxNumberOfMessages + 1, StartDate = DateTime.MinValue, ReadNew = true }
            ).SetDescription($"Invalid NumberOfMessages, must be less or equal than limit ({maxNumberOfMessages}).");
        }

        [Test]
        [TestCaseSource(nameof(GetBadRequestTestCases))]
        public async Task GetSomeCustomEvents_InvalidRequest_ReturnsBadRequest(GetSomeMessagesRequest request)
        {
            // Arrange
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetSomeCustomEvents_WrongKey_ReturnsEmptyArray()
        {
            // Arrange
            var request = new GetSomeMessagesRequest { Key = Guid.NewGuid().ToString(), NumberOfMessages = 1, StartDate = DateTime.MinValue, ReadNew = true };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/analyze/somecustomevents");
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.SendAsync(httpRequest);

            // Assert
            Assert.That(httpResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var content = await httpResponse.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<CustomEventWrapper>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(events);
            Assert.That(events.Count, Is.EqualTo(0));
        }
    }
}
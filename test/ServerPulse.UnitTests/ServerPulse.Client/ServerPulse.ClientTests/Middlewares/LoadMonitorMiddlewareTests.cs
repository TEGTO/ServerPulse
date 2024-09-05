using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServerPulse.Client;
using ServerPulse.Client.Middlewares;
using ServerPulse.Client.Services.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.ClientTests.Middlewares.Tests
{
    [TestFixture]
    internal class LoadMonitorMiddlewareTests
    {
        private Mock<IQueueMessageSender<LoadEvent>> mockServerLoadSender;
        private RequestDelegate next;
        private LoadMonitorMiddleware middleware;
        private DefaultHttpContext httpContext;

        [SetUp]
        public void Setup()
        {
            mockServerLoadSender = new Mock<IQueueMessageSender<LoadEvent>>();
            next = async context => await Task.CompletedTask;
            var configuration = new EventSendingSettings { Key = "testKey", EventController = "http://example.com" };
            middleware = new LoadMonitorMiddleware(next, mockServerLoadSender.Object, configuration);

            httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/test";
            httpContext.Request.Method = "GET";
            httpContext.Response.StatusCode = 200;
        }

        [Test]
        public async Task InvokeAsync_CallsNextMiddlewareAndSendsLoadEvent()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            httpContext.Response.Body = new MemoryStream();
            var timeDelay = TimeSpan.FromMilliseconds(50);
            // Act
            await Task.Delay(timeDelay);
            await middleware.InvokeAsync(httpContext);
            var endTime = DateTime.UtcNow;
            // Assert
            mockServerLoadSender.Verify(x => x.SendEvent(It.Is<LoadEvent>(e =>
                e.Key == "testKey" &&
                e.Endpoint == "/test" &&
                e.Method == "GET" &&
                e.StatusCode == 200
            )), Times.Once);
        }
        [Test]
        public async Task InvokeAsync_HandlesEmptyBodyCorrectly()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            httpContext.Response.Body = new MemoryStream();
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            // Act
            await Task.Delay(0);
            await middleware.InvokeAsync(httpContext);
            var endTime = DateTime.UtcNow;
            // Assert
            mockServerLoadSender.Verify(x => x.SendEvent(It.Is<LoadEvent>(e =>
             e.Key == "testKey" &&
             e.Endpoint == "/test" &&
             e.Method == "GET" &&
             e.StatusCode == 200
            )), Times.Once);
        }
        [Test]
        public async Task UseLoadMonitor_AddsMiddlewareToPipeline()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(mockServerLoadSender.Object);
            services.AddSingleton(new EventSendingSettings { Key = "testKey", EventController = "http://example.com" });
            var serviceProvider = services.BuildServiceProvider();
            // Act
            var appBuilder = new ApplicationBuilder(serviceProvider);
            appBuilder.UseLoadMonitor();
            // Assert
            Assert.NotNull(appBuilder);
        }
    }
}

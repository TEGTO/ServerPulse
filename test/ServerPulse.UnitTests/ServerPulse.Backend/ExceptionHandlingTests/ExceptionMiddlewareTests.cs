using EntityFramework.Exceptions.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Net;

namespace ExceptionHandling.Tests
{
    [TestFixture]
    internal class ExceptionMiddlewareTests
    {
        private Mock<ILogger<ExceptionMiddleware>> loggerMock;
        private DefaultHttpContext httpContext;

        [SetUp]
        public void Setup()
        {
            loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
            httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
        }

        private ExceptionMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new ExceptionMiddleware(next, loggerMock.Object);
        }

        [Test]
        [TestCase(typeof(ValidationException), HttpStatusCode.BadRequest, "Validation error.")]
        [TestCase(typeof(InvalidDataException), HttpStatusCode.BadRequest, "Invalid data error.")]
        [TestCase(typeof(UniqueConstraintException), HttpStatusCode.Conflict, "Constraint error.")]
        [TestCase(typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized, "Invalid Authentication.")]
        [TestCase(typeof(InvalidOperationException), HttpStatusCode.Conflict, "Operation error.")]
        [TestCase(typeof(AuthorizationException), HttpStatusCode.Conflict, "Authorization error occurred.")]
        [TestCase(typeof(SecurityTokenMalformedException), HttpStatusCode.Conflict, "Security token error.")]
        [TestCase(typeof(Exception), HttpStatusCode.InternalServerError, "Internal Server Error.")]
        public async Task InvokeAsync_ExceptionThrown_SetsCorrectStatusCodeAndLogsError(Type exceptionType, HttpStatusCode expectedStatusCode, string exceptionMessage)
        {
            // Arrange
            var exception = (Exception)Activator.CreateInstance(exceptionType, exceptionMessage)!;

            RequestDelegate next = context => throw exception;
            var exceptionMiddleware = CreateMiddleware(next);

            // Act
            await exceptionMiddleware.InvokeAsync(httpContext);

            // Assert
            Assert.That(httpContext.Response.StatusCode, Is.EqualTo((int)expectedStatusCode));

            loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }

        [Test]
        [TestCase("Name", "Name is required.", "400")]
        public async Task InvokeAsync_ValidationException_ResponseBodyContainsValidationErrors(string propertyName, string errorMessage, string expectedResponseBody)
        {
            // Arrange
            var errors = new List<FluentValidation.Results.ValidationFailure> { new FluentValidation.Results.ValidationFailure(propertyName, errorMessage) };
            var exception = new ValidationException(errors);

            RequestDelegate next = context => throw exception;
            var exceptionMiddleware = CreateMiddleware(next);

            // Act
            await exceptionMiddleware.InvokeAsync(httpContext);

            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();

            // Assert
            StringAssert.Contains(expectedResponseBody, responseBody);

            loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(1));
        }
    }
}
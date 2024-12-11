using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using Moq;

namespace AuthenticationApi.Command.ChechAuthData.Tests
{
    [TestFixture]
    internal class CheckAuthDataCommandHandlerTests
    {
        private Mock<IUserService> mockUserService;
        private CheckAuthDataCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockUserService = new Mock<IUserService>();
            handler = new CheckAuthDataCommandHandler(mockUserService.Object);
        }

        private static IEnumerable<TestCaseData> CheckAuthDataTestCases()
        {
            var validUser = new User { UserName = "testuser", Email = "testuser@example.com" };
            var validRequest = new CheckAuthDataRequest { Login = "testuser", Password = "validpassword" };

            yield return new TestCaseData(
                validRequest,
                validUser,
                true,
                new CheckAuthDataResponse { IsCorrect = true }
            ).SetDescription("Valid login and password should return IsCorrect = true.");

            yield return new TestCaseData(
                new CheckAuthDataRequest { Login = "testuser", Password = "invalidpassword" },
                validUser,
                false,
                new CheckAuthDataResponse { IsCorrect = false }
            ).SetDescription("Valid login but invalid password should return IsCorrect = false.");

            yield return new TestCaseData(
                new CheckAuthDataRequest { Login = "nonexistentuser", Password = "any" },
                null,
                false,
                new CheckAuthDataResponse { IsCorrect = false }
            ).SetDescription("Non-existent user should return IsCorrect = false.");
        }

        [Test]
        [TestCaseSource(nameof(CheckAuthDataTestCases))]
        public async Task Handle_CheckAuthDataCommand_TestCases(
            CheckAuthDataRequest request,
            User? user,
            bool passwordCheckResult,
            CheckAuthDataResponse expectedResponse)
        {
            // Arrange
            var command = new CheckAuthDataCommand(request);

            mockUserService.Setup(m => m.GetUserByLoginAsync(request.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            if (user != null)
            {
                mockUserService.Setup(m => m.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(passwordCheckResult);
            }

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result.IsCorrect, Is.EqualTo(expectedResponse.IsCorrect));

            mockUserService.Verify(m => m.GetUserByLoginAsync(request.Login, It.IsAny<CancellationToken>()), Times.Once);

            if (user != null)
            {
                mockUserService.Verify(m => m.CheckPasswordAsync(user, request.Password, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using Moq;

namespace AuthenticationApi.Command.LoginUser.Tests
{
    [TestFixture]
    internal class LoginUserCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IUserService> mockUserService;
        private Mock<IMapper> mockMapper;
        private LoginUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockUserService = new Mock<IUserService>();
            mockMapper = new Mock<IMapper>();
            handler = new LoginUserCommandHandler(mockAuthService.Object, mockUserService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            var validUser = new User { UserName = "validuser", Email = "validuser@example.com" };
            var validRequest = new UserAuthenticationRequest { Login = "validuser", Password = "validpassword" };
            var validToken = new AuthToken { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" };
            var validResponse = new UserAuthenticationResponse
            {
                AuthToken = validToken,
                Email = validUser.Email,
                UserName = validUser.UserName
            };

            yield return new TestCaseData(
                validRequest,
                validUser,
                new LoginUserModel(validUser, "validpassword"),
                validToken,
                validResponse
            ).SetDescription("Valid login and password should return correct authentication response.");

            var invalidRequest = new UserAuthenticationRequest { Login = "invaliduser", Password = "invalidpassword" };

            yield return new TestCaseData(
                invalidRequest,
                null,
                null,
                null,
                null
            ).SetDescription("Invalid login should throw an UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task Handle_LoginUserCommand_TestCases(
            UserAuthenticationRequest request,
            User? user,
            LoginUserModel? loginModel,
            AuthToken? token,
            UserAuthenticationResponse? expectedResponse)
        {
            // Arrange
            var command = new LoginUserCommand(request);

            if (user != null)
            {
                mockUserService.Setup(m => m.GetUserByLoginAsync(request.Login, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);
                mockAuthService.Setup(m => m.LoginUserAsync(It.Is<LoginUserModel>(l => l.Password == request.Password), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AccessTokenData { AccessToken = token?.AccessToken!, RefreshToken = token?.RefreshToken! });
                mockMapper.Setup(m => m.Map<AuthToken>(It.IsAny<AccessTokenData>()))
                    .Returns(token!);
            }
            else
            {
                mockUserService.Setup(m => m.GetUserByLoginAsync(request.Login, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            // Act & Assert
            if (user == null)
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
            }
            else
            {
                var result = await handler.Handle(command, CancellationToken.None);

                Assert.IsNotNull(result);
                Assert.IsNotNull(expectedResponse);

                Assert.That(result.AuthToken, Is.EqualTo(expectedResponse.AuthToken));
                Assert.That(result.Email, Is.EqualTo(expectedResponse.Email));
                Assert.That(result.UserName, Is.EqualTo(expectedResponse.UserName));

                mockUserService.Verify(x => x.GetUserByLoginAsync(request.Login, It.IsAny<CancellationToken>()), Times.Once);
                mockAuthService.Verify(x => x.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
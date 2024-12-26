using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Moq;

namespace AuthenticationApi.Command.LoginUser.Tests
{
    [TestFixture]
    internal class LoginUserCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private LoginUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            handler = new LoginUserCommandHandler(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            var validRequest = new UserAuthenticationRequest { Login = "validuser", Password = "validpassword" };
            var validToken = new AccessTokenDataDto { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" };
            var validResponse = new UserAuthenticationResponse
            {
                AuthToken = validToken,
                Email = validRequest.Login, // !IMPORTANT!: Login == Email for this project!
            };

            yield return new TestCaseData(
                validRequest,
                validToken,
                validResponse
            ).SetDescription("Valid login and password should return correct authentication response.");

        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task Handle_LoginUserCommand_TestCases(UserAuthenticationRequest request, AccessTokenDataDto? token, UserAuthenticationResponse? expectedResponse)
        {
            // Arrange
            var command = new LoginUserCommand(request);

            mockAuthService.Setup(m => m.LoginUserAsync(It.Is<LoginUserModel>(l => l.Password == request.Password), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessTokenData { AccessToken = token?.AccessToken!, RefreshToken = token?.RefreshToken! });
            mockMapper.Setup(m => m.Map<AccessTokenDataDto>(It.IsAny<AccessTokenData>()))
                .Returns(token!);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(expectedResponse);

            Assert.That(result.AuthToken, Is.EqualTo(expectedResponse.AuthToken));
            Assert.That(result.Email, Is.EqualTo(expectedResponse.Email));

            mockAuthService.Verify(x => x.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
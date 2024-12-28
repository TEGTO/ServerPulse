using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.FeatureManagement;
using Moq;

namespace AuthenticationApi.Command.LoginUser.Tests
{
    [TestFixture]
    internal class LoginUserCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IFeatureManager> mockFeatureManager;
        private Mock<IMapper> mockMapper;
        private LoginUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockFeatureManager = new Mock<IFeatureManager>();
            mockMapper = new Mock<IMapper>();

            handler = new LoginUserCommandHandler(mockAuthService.Object, mockFeatureManager.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            var validRequest = new UserAuthenticationRequest { Login = "validuser", Password = "validpassword" };
            var validToken = new AccessTokenDataDto { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" };
            var validResponse = new UserAuthenticationResponse
            {
                AuthToken = validToken,
                Email = validRequest.Login, // Login == Email in this project
            };

            yield return new TestCaseData(
                validRequest,
                validToken,
                validResponse,
                true,
                true
            ).SetDescription("Valid login and password with email confirmation enabled and confirmed should return correct authentication response.");

            yield return new TestCaseData(
                validRequest,
                null,
                null,
                true,
                false
            ).SetDescription("Valid login and password with email confirmation enabled but not confirmed should throw UnauthorizedAccessException.");

            yield return new TestCaseData(
                validRequest,
                validToken,
                validResponse,
                false,
                true
            ).SetDescription("Valid login and password with email confirmation disabled should return correct authentication response.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task Handle_LoginUserCommand_TestCases(
            UserAuthenticationRequest request,
            AccessTokenDataDto? token,
            UserAuthenticationResponse? expectedResponse,
            bool emailConfirmationEnabled,
            bool isValid)
        {
            // Arrange
            var command = new LoginUserCommand(request);

            mockFeatureManager.Setup(m => m.IsEnabledAsync(ConfigurationKeys.REQUIRE_EMAIL_CONFIRMATION))
                .ReturnsAsync(emailConfirmationEnabled);

            if (emailConfirmationEnabled)
            {
                mockAuthService.Setup(m => m.CheckEmailConfirmationAsync(request.Login))
                    .ReturnsAsync(isValid);
            }

            if (isValid)
            {
                mockAuthService.Setup(m => m.LoginUserAsync(It.Is<LoginUserModel>(l => l.Login == request.Login && l.Password == request.Password), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AccessTokenData { AccessToken = token?.AccessToken!, RefreshToken = token?.RefreshToken! });
                mockMapper.Setup(m => m.Map<AccessTokenDataDto>(It.IsAny<AccessTokenData>()))
                    .Returns(token!);
            }

            // Act & Assert
            if (!isValid)
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

                mockAuthService.Verify(x => x.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()), Times.Once);
                if (emailConfirmationEnabled)
                {
                    mockAuthService.Verify(x => x.CheckEmailConfirmationAsync(request.Login), Times.Once);
                }
            }
        }
    }
}
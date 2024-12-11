using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthenticationApi.Command.RegisterUser.Tests
{
    [TestFixture]
    internal class RegisterUserCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private RegisterUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();
            handler = new RegisterUserCommandHandler(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> RegisterUserTestCases()
        {
            var validRequest = new UserRegistrationRequest
            {
                Email = "validuser@example.com",
                Password = "validpassword",
                ConfirmPassword = "validpassword"
            };

            var validUser = new User { Email = validRequest.Email };

            var validRegisterModel = new RegisterUserModel(validUser, validRequest.Password);

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
                validRegisterModel,
                IdentityResult.Success,
                validToken,
                validResponse,
                true
            ).SetDescription("Valid user registration should return authentication response.");

            var invalidRequest = new UserRegistrationRequest
            {
                Email = "invaliduser@example.com",
                Password = "weakpassword",
                ConfirmPassword = "weakpassword"
            };

            var invalidUser = new User { Email = invalidRequest.Email };

            yield return new TestCaseData(
                invalidRequest,
                invalidUser,
                new RegisterUserModel(invalidUser, invalidRequest.Password),
                IdentityResult.Failed(new IdentityError { Description = "Invalid password" }),
                null,
                null,
                false
            ).SetDescription("Invalid user registration should throw AuthorizationException.");
        }

        [Test]
        [TestCaseSource(nameof(RegisterUserTestCases))]
        public async Task Handle_RegisterUserCommand_TestCases(
            UserRegistrationRequest request,
            User user,
            RegisterUserModel registerModel,
            IdentityResult registerResult,
            AuthToken? token,
            UserAuthenticationResponse? expectedResponse,
            bool isValid)
        {
            // Arrange
            var command = new RegisterUserCommand(request);

            mockMapper.Setup(m => m.Map<User>(request)).Returns(user);
            mockAuthService.Setup(m => m.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(registerResult);

            if (isValid)
            {
                mockAuthService.Setup(m => m.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AccessTokenData { AccessToken = token?.AccessToken!, RefreshToken = token?.RefreshToken! });

                mockMapper.Setup(m => m.Map<AuthToken>(It.IsAny<AccessTokenData>())).Returns(token!);
            }

            // Act & Assert
            if (!isValid)
            {
                Assert.ThrowsAsync<AuthorizationException>(() => handler.Handle(command, CancellationToken.None));
            }
            else
            {
                var result = await handler.Handle(command, CancellationToken.None);

                Assert.IsNotNull(result);
                Assert.IsNotNull(expectedResponse);

                Assert.That(result.AuthToken, Is.EqualTo(expectedResponse.AuthToken));
                Assert.That(result.Email, Is.EqualTo(expectedResponse.Email));
                Assert.That(result.UserName, Is.EqualTo(expectedResponse.UserName));

                mockAuthService.Verify(x => x.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()), Times.Once);
                mockAuthService.Verify(x => x.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()), Times.Once);
                mockMapper.Verify(x => x.Map<AuthToken>(It.IsAny<AccessTokenData>()), Times.Once);
            }
        }
    }
}
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using BackgroundTask;
using ExceptionHandling;
using Microsoft.AspNetCore.Identity;
using Microsoft.FeatureManagement;
using Moq;
using System.Linq.Expressions;
using IEmailSender = EmailControl.IEmailSender;

namespace AuthenticationApi.Command.RegisterUser.Tests
{
    [TestFixture]
    internal class RegisterUserCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private Mock<IEmailSender> mockEmailSender;
        private Mock<IFeatureManager> mockFeatureManager;
        private Mock<IBackgroundJobClient> mockBackgroundJobClient;
        private RegisterUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();
            mockEmailSender = new Mock<IEmailSender>();
            mockFeatureManager = new Mock<IFeatureManager>();
            mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            handler = new RegisterUserCommandHandler(
                mockAuthService.Object,
                mockFeatureManager.Object,
                mockEmailSender.Object,
                mockBackgroundJobClient.Object,
                mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> RegisterUserTestCases()
        {
            var validRequest = new UserRegistrationRequest
            {
                Email = "validuser@example.com",
                Password = "validpassword",
                ConfirmPassword = "validpassword",
                RedirectConfirmUrl = "https://example.com/confirm"
            };

            var validUser = new User { Email = validRequest.Email };

            var validRegisterModel = new RegisterUserModel { User = validUser, Password = validRequest.Password };

            yield return new TestCaseData(
                validRequest,
                validUser,
                validRegisterModel,
                IdentityResult.Success,
                true,
                true
            ).SetDescription("Valid user registration with email confirmation enabled should succeed.");

            yield return new TestCaseData(
                validRequest,
                validUser,
                validRegisterModel,
                IdentityResult.Success,
                false,
                true
            ).SetDescription("Valid user registration with email confirmation disabled should succeed.");

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
                new RegisterUserModel { User = invalidUser, Password = invalidRequest.Password },
                IdentityResult.Failed(new IdentityError { Description = "Invalid password" }),
                true,
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
            bool emailConfirmationEnabled,
            bool isValid)
        {
            // Arrange
            var command = new RegisterUserCommand(request);

            mockMapper.Setup(m => m.Map<User>(request)).Returns(user);
            mockAuthService.Setup(m => m.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(registerResult);
            mockFeatureManager.Setup(m => m.IsEnabledAsync(ConfigurationKeys.REQUIRE_EMAIL_CONFIRMATION))
                .ReturnsAsync(emailConfirmationEnabled);

            if (emailConfirmationEnabled)
            {
                mockAuthService.Setup(m => m.GetEmailConfirmationTokenAsync(request.Email))
                    .ReturnsAsync("test-token");
            }

            // Act & Assert
            if (!isValid)
            {
                Assert.ThrowsAsync<AuthorizationException>(() => handler.Handle(command, CancellationToken.None));
            }
            else
            {
                await handler.Handle(command, CancellationToken.None);

                Assert.Pass();

                mockAuthService.Verify(x => x.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()), Times.Once);

                if (emailConfirmationEnabled)
                {
                    mockAuthService.Verify(x => x.GetEmailConfirmationTokenAsync(request.Email), Times.Once);
                    mockEmailSender.Verify(x => x.SendEmailAsync(
                        request.Email,
                        "[Server Pulse] Confirm Your Email Address",
                        It.Is<string>(body => body.Contains(request.RedirectConfirmUrl)),
                        It.IsAny<CancellationToken>()), Times.Once);

                    mockBackgroundJobClient.Verify(x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()), Times.Once);
                }
                else
                {
                    mockAuthService.Verify(x => x.GetEmailConfirmationTokenAsync(request.Email), Times.Never);
                    mockEmailSender.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
                }
            }
        }
    }
}
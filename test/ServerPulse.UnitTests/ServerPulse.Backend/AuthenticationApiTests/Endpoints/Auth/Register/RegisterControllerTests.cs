using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Moq;

namespace AuthenticationApi.Endpoints.Auth.Register.Tests
{
    [TestFixture]
    internal class RegisterControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private Mock<IEmailJobService> mockEmailJobService;
        private Mock<IFeatureManager> mockFeatureManager;
        private Mock<IBackgroundJobClient> mockBackgroundJobClient;
        private RegisterController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();
            mockEmailJobService = new Mock<IEmailJobService>();
            mockFeatureManager = new Mock<IFeatureManager>();
            mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            controller = new RegisterController(
                mockAuthService.Object,
                mockFeatureManager.Object,
                mockBackgroundJobClient.Object,
                mockEmailJobService.Object,
                mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> RegisterUserTestCases()
        {
            var validRequest = new RegisterRequest
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

            var invalidRequest = new RegisterRequest
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
        public async Task Register_TestCases(
            RegisterRequest request,
            User user,
            RegisterUserModel registerModel,
            IdentityResult registerResult,
            bool emailConfirmationEnabled,
            bool isValid)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<User>(request)).Returns(user);

            mockAuthService.Setup(m => m.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(registerResult);

            mockFeatureManager.Setup(m => m.IsEnabledAsync(Features.EMAIL_CONFIRMATION))
                .ReturnsAsync(emailConfirmationEnabled);

            if (emailConfirmationEnabled)
            {
                mockAuthService.Setup(m => m.GetEmailConfirmationTokenAsync(request.Email))
                    .ReturnsAsync("test-token");
            }

            if (!isValid)
            {
                // Act
                var result = await controller.Register(request, CancellationToken.None);

                // Assert
                Assert.IsInstanceOf<ConflictObjectResult>(result);
            }
            else
            {
                // Act
                await controller.Register(request, CancellationToken.None);

                // Assert
                mockAuthService.Verify(x => x.RegisterUserAsync(registerModel, It.IsAny<CancellationToken>()), Times.Once);

                if (emailConfirmationEnabled)
                {
                    mockBackgroundJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Once);
                }
                else
                {
                    mockBackgroundJobClient.Verify(x => x.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
                }
            }
        }
    }
}
using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Moq;

namespace AuthenticationApi.Endpoints.Auth.Login.Tests
{
    [TestFixture]
    internal class LoginControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IFeatureManager> mockFeatureManager;
        private Mock<IMapper> mockMapper;
        private LoginController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockFeatureManager = new Mock<IFeatureManager>();
            mockMapper = new Mock<IMapper>();

            controller = new LoginController(mockAuthService.Object, mockFeatureManager.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            var validRequest = new LoginRequest { Login = "validuser", Password = "validpassword" };
            var validToken = new AccessTokenDataDto { AccessToken = "valid_token", RefreshToken = "valid_refresh_token" };
            var validResponse = new LoginResponse
            {
                AccessTokenData = validToken,
                Email = validRequest.Login,
            };

            yield return new TestCaseData(
                validRequest,
                validToken,
                validResponse,
                true,
                true
            ).SetDescription("Valid login and password with email confirmation enabled and confirmed should return correct Ok response.");

            yield return new TestCaseData(
                validRequest,
                null,
                null,
                true,
                false
            ).SetDescription("Valid login and password with email confirmation enabled but not confirmed should return Unauthorized response.");

            yield return new TestCaseData(
                validRequest,
                validToken,
                validResponse,
                false,
                true
            ).SetDescription("Valid login and password with email confirmation disabled should return correct Ok response.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task Login_TestCases(
            LoginRequest request,
            AccessTokenDataDto? token,
            LoginResponse? expectedResponse,
            bool emailConfirmationEnabled,
            bool isValid)
        {
            // Arrange
            mockFeatureManager.Setup(m => m.IsEnabledAsync(Features.EMAIL_CONFIRMATION))
                .ReturnsAsync(emailConfirmationEnabled);

            if (emailConfirmationEnabled)
            {
                mockAuthService.Setup(m => m.CheckEmailConfirmationAsync(request.Login))
                    .ReturnsAsync(isValid);
            }

            if (isValid)
            {
                mockAuthService.Setup(m => m.LoginUserAsync(
                    It.Is<LoginUserModel>(l => l.Login == request.Login && l.Password == request.Password),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new AccessTokenData { AccessToken = token?.AccessToken!, RefreshToken = token?.RefreshToken! });

                mockMapper.Setup(m => m.Map<AccessTokenDataDto>(It.IsAny<AccessTokenData>()))
                    .Returns(token!);
            }

            if (!isValid)
            {
                // Act
                var result = await controller.Login(request, CancellationToken.None);

                // Assert
                Assert.IsInstanceOf<UnauthorizedObjectResult>(result.Result);
            }
            else
            {
                // Act
                var result = await controller.Login(request, CancellationToken.None);

                // Assert
                Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());

                var response = (result.Result as OkObjectResult)?.Value as LoginResponse;

                Assert.NotNull(response);
                Assert.IsNotNull(expectedResponse);

                Assert.That(response.AccessTokenData, Is.EqualTo(expectedResponse.AccessTokenData));
                Assert.That(response.Email, Is.EqualTo(expectedResponse.Email));

                mockAuthService.Verify(x => x.LoginUserAsync(It.IsAny<LoginUserModel>(), It.IsAny<CancellationToken>()), Times.Once);
                if (emailConfirmationEnabled)
                {
                    mockAuthService.Verify(x => x.CheckEmailConfirmationAsync(request.Login), Times.Once);
                }
            }
        }
    }
}
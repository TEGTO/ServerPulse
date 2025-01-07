using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Endpoints.Auth.ConfirmEmail.Tests
{
    [TestFixture]
    internal class ConfirmEmailControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private ConfirmEmailController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            controller = new ConfirmEmailController(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> ConfirmEmailTestCases()
        {
            var validRequest = new ConfirmEmailRequest
            {
                Email = "validuser@example.com",
                Token = "valid_token"
            };

            var validAccessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(60)
            };

            var validAccessTokenDto = new AccessTokenDataDto
            {
                AccessToken = validAccessTokenData.AccessToken,
                RefreshToken = validAccessTokenData.RefreshToken,
                RefreshTokenExpiryDate = validAccessTokenData.RefreshTokenExpiryDate
            };

            var validResponse = new ConfirmEmailResponse
            {
                AccessTokenData = validAccessTokenDto,
                Email = validRequest.Email
            };

            yield return new TestCaseData(
                validRequest,
                IdentityResult.Success,
                validAccessTokenData,
                validAccessTokenDto,
                validResponse,
                true
            ).SetDescription("Valid email confirmation and login should return Ok response.");

            yield return new TestCaseData(
                validRequest,
                IdentityResult.Failed(new IdentityError { Description = "Invalid token" }),
                null,
                null,
                null,
                false
            ).SetDescription("Invalid email confirmation token should return Unauthorized response.");
        }

        [Test]
        [TestCaseSource(nameof(ConfirmEmailTestCases))]
        public async Task ConfirmEmail_TestCases(
            ConfirmEmailRequest request,
            IdentityResult confirmResult,
            AccessTokenData? accessTokenData,
            AccessTokenDataDto? accessTokenDto,
            ConfirmEmailResponse? expectedResponse,
            bool isValid)
        {
            // Arrange
            mockAuthService.Setup(x => x.ConfirmEmailAsync(request.Email, request.Token))
                .ReturnsAsync(confirmResult);

            if (isValid)
            {
                mockAuthService.Setup(x => x.LoginUserAfterConfirmationAsync(request.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessTokenData!);

                mockMapper.Setup(x => x.Map<AccessTokenDataDto>(accessTokenData!))
                    .Returns(accessTokenDto!);
            }

            if (!isValid)
            {
                // Act
                var result = await controller.ConfirmEmail(request, CancellationToken.None);

                // Assert
                Assert.IsInstanceOf<ConflictObjectResult>(result.Result);
            }
            else
            {
                // Act
                var result = await controller.ConfirmEmail(request, CancellationToken.None);

                // Assert
                Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
                var response = (result.Result as OkObjectResult)?.Value as ConfirmEmailResponse;
                Assert.NotNull(response);

                Assert.That(response.AccessTokenData, Is.EqualTo(expectedResponse?.AccessTokenData));
                Assert.That(response.Email, Is.EqualTo(expectedResponse?.Email));

                mockAuthService.Verify(x => x.ConfirmEmailAsync(request.Email, request.Token), Times.Once);
                mockAuthService.Verify(x => x.LoginUserAfterConfirmationAsync(request.Email, It.IsAny<CancellationToken>()), Times.Once);
                mockMapper.Verify(x => x.Map<AccessTokenDataDto>(It.IsAny<AccessTokenData>()), Times.Once);
            }
        }
    }
}
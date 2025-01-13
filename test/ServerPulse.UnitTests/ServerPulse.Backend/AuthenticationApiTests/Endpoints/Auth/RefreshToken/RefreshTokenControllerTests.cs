using Authentication.Models;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Endpoints.Auth.RefreshToken.Tests
{
    [TestFixture]
    internal class RefreshTokenControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private RefreshTokenController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            controller = new RefreshTokenController(
                mockAuthService.Object,
                mockMapper.Object
            );
        }

        private static IEnumerable<TestCaseData> RefreshTokenTestCases()
        {
            var validRequest = new RefreshTokenRequest
            {
                AccessToken = "valid_access_token",
                RefreshToken = "valid_refresh_token",
                RefreshTokenExpiryDate = DateTime.MaxValue
            };

            var validAccessTokenData = new AccessTokenData
            {
                AccessToken = validRequest.AccessToken,
                RefreshToken = validRequest.RefreshToken,
                RefreshTokenExpiryDate = validRequest?.RefreshTokenExpiryDate ?? DateTime.MaxValue
            };

            var newAccessTokenData = new AccessTokenData
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                RefreshTokenExpiryDate = DateTime.MaxValue
            };

            var expectedResponse = new RefreshTokenResponse
            {
                AccessToken = newAccessTokenData.AccessToken,
                RefreshToken = newAccessTokenData.RefreshToken,
                RefreshTokenExpiryDate = newAccessTokenData.RefreshTokenExpiryDate
            };

            yield return new TestCaseData(
                validRequest,
                validAccessTokenData,
                newAccessTokenData,
                expectedResponse
            ).SetDescription("Valid refresh token should return a new token Ok response.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshTokenTestCases))]
        public async Task RefreshToken_TestCases(
            RefreshTokenRequest request,
            AccessTokenData accessTokenData,
            AccessTokenData? newAccessTokenData,
            RefreshTokenResponse? expectedResponse)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<AccessTokenData>(request)).Returns(accessTokenData);
            mockMapper.Setup(m => m.Map<RefreshTokenResponse>(newAccessTokenData)).Returns(expectedResponse!);

            mockAuthService.Setup(m => m.RefreshTokenAsync(It.IsAny<AccessTokenData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newAccessTokenData!);

            // Act
            var result = await controller.RefreshToken(request, CancellationToken.None);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var response = (result.Result as OkObjectResult)?.Value as RefreshTokenResponse;
            Assert.NotNull(response);

            Assert.That(response, Is.EqualTo(expectedResponse));
        }
    }
}
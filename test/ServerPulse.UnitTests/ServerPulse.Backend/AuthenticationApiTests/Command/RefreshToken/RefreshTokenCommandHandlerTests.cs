using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Services;
using AutoMapper;
using Moq;

namespace AuthenticationApi.Command.RefreshToken.Tests
{
    [TestFixture]
    internal class RefreshTokenCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private RefreshTokenCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            handler = new RefreshTokenCommandHandler(
                mockAuthService.Object,
                mockMapper.Object
            );
        }

        private static IEnumerable<TestCaseData> RefreshTokenTestCases()
        {
            var validAuthToken = new AccessTokenDataDto
            {
                AccessToken = "valid_access_token",
                RefreshToken = "valid_refresh_token",
                RefreshTokenExpiryDate = DateTime.MaxValue
            };

            var validAccessTokenData = new AccessTokenData
            {
                AccessToken = validAuthToken.AccessToken,
                RefreshToken = validAuthToken.RefreshToken,
                RefreshTokenExpiryDate = validAuthToken?.RefreshTokenExpiryDate ?? DateTime.MaxValue
            };

            var newAccessTokenData = new AccessTokenData
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                RefreshTokenExpiryDate = DateTime.MaxValue
            };

            var expectedResponse = new AccessTokenDataDto
            {
                AccessToken = newAccessTokenData.AccessToken,
                RefreshToken = newAccessTokenData.RefreshToken,
                RefreshTokenExpiryDate = newAccessTokenData.RefreshTokenExpiryDate
            };

            yield return new TestCaseData(
                new RefreshTokenCommand(validAuthToken!),
                validAccessTokenData,
                newAccessTokenData,
                expectedResponse
            ).SetDescription("Valid refresh token should return a new token response.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshTokenTestCases))]
        public async Task Handle_RefreshTokenCommand_TestCases(
            RefreshTokenCommand command,
            AccessTokenData accessTokenData,
            AccessTokenData? newAccessTokenData,
            AccessTokenDataDto? expectedResponse)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<AccessTokenData>(command.Request)).Returns(accessTokenData);
            mockMapper.Setup(m => m.Map<AccessTokenDataDto>(newAccessTokenData)).Returns(expectedResponse!);

            mockAuthService.Setup(m => m.RefreshTokenAsync(It.IsAny<AccessTokenData>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newAccessTokenData!);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResponse));
        }
    }
}
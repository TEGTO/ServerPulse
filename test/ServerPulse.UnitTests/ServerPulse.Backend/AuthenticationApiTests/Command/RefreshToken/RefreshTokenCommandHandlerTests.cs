using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Command.RefreshToken.Tests
{
    [TestFixture]
    internal class RefreshTokenCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<ITokenService> mockTokenService;
        private Mock<IUserService> mockUserService;
        private Mock<IMapper> mockMapper;
        private RefreshTokenCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockTokenService = new Mock<ITokenService>();
            mockUserService = new Mock<IUserService>();
            mockMapper = new Mock<IMapper>();

            handler = new RefreshTokenCommandHandler(
                mockAuthService.Object,
                mockUserService.Object,
                mockTokenService.Object,
                mockMapper.Object
            );
        }

        private static IEnumerable<TestCaseData> RefreshTokenTestCases()
        {
            var validAuthToken = new AuthToken
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

            var validUser = new User
            {
                Id = "user-id",
                UserName = "testuser",
                RefreshToken = validAuthToken!.RefreshToken
            };

            var newAccessTokenData = new AccessTokenData
            {
                AccessToken = "new_access_token",
                RefreshToken = "new_refresh_token",
                RefreshTokenExpiryDate = DateTime.MaxValue
            };

            var newAuthToken = new AuthToken
            {
                AccessToken = newAccessTokenData.AccessToken,
                RefreshToken = newAccessTokenData.RefreshToken,
                RefreshTokenExpiryDate = newAccessTokenData.RefreshTokenExpiryDate
            };

            yield return new TestCaseData(
                new RefreshTokenCommand(validAuthToken),
                validAccessTokenData,
                validUser,
                newAccessTokenData,
                newAuthToken,
                true
            ).SetDescription("Valid refresh token should return a new AuthToken.");

            yield return new TestCaseData(
                new RefreshTokenCommand(validAuthToken),
                validAccessTokenData,
                null,
                null,
                null,
                false
            ).SetDescription("User not found by access token should throw an exception.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshTokenTestCases))]
        public async Task Handle_RefreshTokenCommand_TestCases(
            RefreshTokenCommand command,
            AccessTokenData accessTokenData,
            User? user,
            AccessTokenData? newAccessTokenData,
            AuthToken? expectedAuthToken,
            bool isValid)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<AccessTokenData>(command.Request)).Returns(accessTokenData);

            if (isValid)
            {
                mockTokenService.Setup(m => m.GetPrincipalFromToken(accessTokenData.AccessToken))
                    .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user?.Id!) })));

                mockUserService.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);

                mockAuthService.Setup(m => m.RefreshTokenAsync(It.IsAny<RefreshTokenModel>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newAccessTokenData!);

                mockMapper.Setup(m => m.Map<AuthToken>(newAccessTokenData)).Returns(expectedAuthToken!);
            }
            else
            {

                mockUserService.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            // Act & Assert
            if (isValid)
            {
                var result = await handler.Handle(command, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedAuthToken));
            }
            else
            {
                Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
            }
        }
    }
}
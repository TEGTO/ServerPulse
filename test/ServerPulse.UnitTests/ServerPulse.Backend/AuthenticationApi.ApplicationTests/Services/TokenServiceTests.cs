using Authentication.Models;
using Authentication.Token;
using AuthenticationApi.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Application.Services.Tests
{
    [TestFixture]
    internal class TokenServiceTests
    {
        private Mock<ITokenHandler> tokenHandlerMock;
        private Mock<UserManager<User>> userManagerMock;
        private Mock<IConfiguration> configurationMock;
        private TokenService tokenService;

        [SetUp]
        public void SetUp()
        {
            tokenHandlerMock = new Mock<ITokenHandler>();
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[ConfigurationKeys.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS]).Returns("7");

            tokenService = new TokenService(tokenHandlerMock.Object, userManagerMock.Object, configurationMock.Object);
        }

        private static IEnumerable<TestCaseData> GenerateTokenTestCases()
        {
            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "some@email", RefreshToken = null },
                true,
                true
            ).SetDescription("Generate token for a new user without refresh token.");

            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "some@email", RefreshToken = "existing-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1) },
                false,
                true
            ).SetDescription("Generate token for a user with valid refresh token.");

            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "some@email", RefreshToken = "expired-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(-1) },
                true,
                false
            ).SetDescription("Generate token for a user with expired refresh token but fails to update.");

            yield return new TestCaseData(
               new User { RefreshToken = "expired-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(-1) },
               true,
               false
           ).SetDescription("Generate token for a user with invalid claims refresh token but fails to generate.");
        }

        [Test]
        [TestCaseSource(nameof(GenerateTokenTestCases))]
        public async Task GenerateTokenAsync_TestCases(User user, bool shouldUpdate, bool updateSuccess)
        {
            // Arrange
            var tokenData = new AccessTokenData { AccessToken = "new-token", RefreshToken = "new-refresh" };
            tokenHandlerMock.Setup(t => t.CreateToken(It.IsAny<IEnumerable<Claim>>())).Returns(tokenData);

            if (shouldUpdate)
            {
                userManagerMock.Setup(u => u.UpdateAsync(user))
                    .ReturnsAsync(updateSuccess ? IdentityResult.Success : IdentityResult.Failed(new IdentityError { Description = "Update failed." }));
            }

            // Act & Assert
            if (shouldUpdate && !updateSuccess)
            {
                Assert.ThrowsAsync(Is.AssignableTo(typeof(Exception)), () => tokenService.GenerateTokenAsync(user, CancellationToken.None));
            }
            else
            {
                var result = await tokenService.GenerateTokenAsync(user, CancellationToken.None);

                Assert.That(result.AccessToken, Is.EqualTo(tokenData.AccessToken));
                Assert.That(result.RefreshToken, Is.EqualTo(tokenData.RefreshToken));

                tokenHandlerMock.Verify(x => x.CreateToken(It.IsAny<IEnumerable<Claim>>()), Times.Once);
            }
        }

        [Test]
        public void GetPrincipalFromExpiredToken_ValidToken_ReturnsPrincipal()
        {
            // Arrange
            var token = "expired-token";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "userId123") }));
            tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(token)).Returns(claimsPrincipal);

            // Act
            var result = tokenService.GetPrincipalFromExpiredToken(token);

            // Assert
            Assert.That(result, Is.EqualTo(claimsPrincipal));
        }

        private static IEnumerable<TestCaseData> RefreshAccessTokenTestCases()
        {
            yield return new TestCaseData(
                new AccessTokenData { AccessToken = "expired-token", RefreshToken = "valid-refresh" },
                new User { UserName = "testuser", Email = "some@email", RefreshToken = "valid-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1) },
                true
            ).SetDescription("Valid refresh token refreshes access token.");

            yield return new TestCaseData(
                new AccessTokenData { AccessToken = "expired-token", RefreshToken = "invalid-refresh" },
                new User { UserName = "testuser", Email = "some@email", RefreshToken = "valid-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1) },
                false
            ).SetDescription("Invalid refresh token throws UnauthorizedAccessException.");

            yield return new TestCaseData(
                new AccessTokenData { AccessToken = "expired-token", RefreshToken = "valid-refresh" },
                new User { UserName = "testuser", Email = "some@email", RefreshToken = "valid-refresh", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(-1) },
                false
            ).SetDescription("Expired refresh token throws UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshAccessTokenTestCases))]
        public async Task RefreshAccessTokenAsync_TestCases(AccessTokenData tokenData, User user, bool isValid)
        {
            // Arrange
            userManagerMock.Setup(u => u.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

            tokenHandlerMock.Setup(t => t.GetPrincipalFromExpiredToken(tokenData.AccessToken))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.UserName!)])));

            if (isValid)
            {
                tokenHandlerMock.Setup(t => t.CreateToken(It.IsAny<IEnumerable<Claim>>()))
                    .Returns(new AccessTokenData { AccessToken = "new-access-token", RefreshToken = "new-refresh-token" });
            }

            // Act & Assert
            if (isValid)
            {
                var result = await tokenService.RefreshAccessTokenAsync(tokenData, user, CancellationToken.None);
                Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
                Assert.That(result.RefreshToken, Is.EqualTo("valid-refresh"));

                tokenHandlerMock.Verify(x => x.CreateToken(It.IsAny<IEnumerable<Claim>>()), Times.Once);
            }
            else
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => tokenService.RefreshAccessTokenAsync(tokenData, user, CancellationToken.None));
            }
        }
    }
}
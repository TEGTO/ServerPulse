using Authentication.Models;
using Authentication.Token;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Services.Tests
{
    [TestFixture]
    internal class TokenServiceTests
    {
        private Mock<ITokenHandler> mockTokenHandler;
        private Mock<UserManager<User>> mockUserManager;
        private TokenService tokenService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();

            mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            mockTokenHandler = new Mock<ITokenHandler>();
            tokenService = new TokenService(mockTokenHandler.Object, mockUserManager.Object);
        }

        private static IEnumerable<TestCaseData> CreateNewTokenDataTestCases()
        {
            var user = new User { UserName = "testuser" };

            yield return new TestCaseData(
                user,
                DateTime.MaxValue,
                new AccessTokenData
                {
                    AccessToken = "test_access_token",
                    RefreshToken = "test_refresh_token",
                    RefreshTokenExpiryDate = DateTime.MaxValue
                }
            ).SetDescription("Valid user with roles should return correct token data.");

            yield return new TestCaseData(
                user,
                DateTime.MaxValue,
                new AccessTokenData
                {
                    AccessToken = "another_test_access_token",
                    RefreshToken = "another_test_refresh_token",
                    RefreshTokenExpiryDate = DateTime.MaxValue
                }
            ).SetDescription("Different expiry date should reflect in token data.");
        }

        [Test]
        [TestCaseSource(nameof(CreateNewTokenDataTestCases))]
        public async Task CreateNewTokenDataAsync_TestCases(User user, DateTime expiryDate, AccessTokenData expectedTokenData)
        {
            // Arrange
            mockTokenHandler.Setup(m => m.CreateToken(user)).Returns(expectedTokenData);

            // Act
            var result = await tokenService.CreateNewTokenDataAsync(user, expiryDate, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedTokenData));
            Assert.That(result.RefreshTokenExpiryDate, Is.EqualTo(expiryDate));
        }

        private static IEnumerable<TestCaseData> SetRefreshTokenTestCases()
        {
            var user = new User { UserName = "testuser" };

            yield return new TestCaseData(
                user,
                new AccessTokenData
                {
                    AccessToken = "access_token",
                    RefreshToken = "refresh_token_1",
                    RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(5)
                },
                IdentityResult.Success
            ).SetDescription("Valid user and token data should update refresh token successfully.");

            yield return new TestCaseData(
                user,
                new AccessTokenData
                {
                    AccessToken = "access_token",
                    RefreshToken = "refresh_token_2",
                    RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(10)
                },
                IdentityResult.Failed(new IdentityError { Description = "Update failed" })
            ).SetDescription("Failed identity update should result in no token update.");
        }

        [Test]
        [TestCaseSource(nameof(SetRefreshTokenTestCases))]
        public async Task SetRefreshTokenAsync_TestCases(User user, AccessTokenData accessTokenData, IdentityResult updateResult)
        {
            // Arrange
            mockUserManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(updateResult);

            // Act
            await tokenService.SetRefreshTokenAsync(user, accessTokenData, CancellationToken.None);

            // Assert
            mockUserManager.Verify(m => m.UpdateAsync(It.Is<User>(u => u.RefreshToken == accessTokenData.RefreshToken && u.RefreshTokenExpiryTime == accessTokenData.RefreshTokenExpiryDate)), Times.Once);
        }

        private static IEnumerable<TestCaseData> GetPrincipalFromTokenTestCases()
        {
            var validToken = "valid_token";
            var expiredToken = "expired_token";

            yield return new TestCaseData(
                validToken,
                new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "testuser") }))
            ).SetDescription("Valid token should return the correct ClaimsPrincipal.");

            yield return new TestCaseData(
                expiredToken,
                new ClaimsPrincipal(new ClaimsIdentity())
            ).SetDescription("Expired token should return an empty ClaimsPrincipal.");
        }

        [Test]
        [TestCaseSource(nameof(GetPrincipalFromTokenTestCases))]
        public void GetPrincipalFromToken_TestCases(string token, ClaimsPrincipal expectedPrincipal)
        {
            // Arrange
            mockTokenHandler.Setup(m => m.GetPrincipalFromExpiredToken(token)).Returns(expectedPrincipal);

            // Act
            var result = tokenService.GetPrincipalFromToken(token);

            // Assert
            Assert.That(result, Is.EqualTo(expectedPrincipal));
        }
    }
}
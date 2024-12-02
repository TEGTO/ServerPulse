using Authentication.Models;
using AuthenticationApi.Infrastructure;
using ExceptionHandling;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AuthenticationApi.Services.Tests
{
    [TestFixture]
    internal class AuthServiceTests
    {
        private Mock<UserManager<User>> userManagerMock;
        private Mock<ITokenService> tokenServiceMock;
        private Mock<IConfiguration> configurationMock;
        private AuthService authService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();

            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            tokenServiceMock = new Mock<ITokenService>();

            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c[It.Is<string>(s => s == Configuration.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS)])
                .Returns("7");

            authService = new AuthService(userManagerMock.Object, tokenServiceMock.Object, configurationMock.Object);
        }

        private static IEnumerable<TestCaseData> RegisterUserTestCases()
        {
            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "test@example.com" },
                "Password123",
                IdentityResult.Success,
                true
            ).SetDescription("Registering a user successfully returns success.");

            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "test@example.com" },
                "WeakPassword",
                IdentityResult.Failed(),
                false
            ).SetDescription("Registering a user with invalid password returns failure.");
        }

        [Test]
        [TestCaseSource(nameof(RegisterUserTestCases))]
        public async Task RegisterUserAsync_TestCases(User user, string password, IdentityResult identityResult, bool isSuccess)
        {
            // Arrange
            var registerParams = new RegisterUserModel(user, password);

            userManagerMock.Setup(x => x.CreateAsync(user, password)).ReturnsAsync(identityResult);

            // Act
            var result = await authService.RegisterUserAsync(registerParams, CancellationToken.None);

            // Assert
            Assert.That(result.Succeeded, Is.EqualTo(isSuccess));
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "test@example.com" },
                "CorrectPassword",
                true,
                new AccessTokenData { AccessToken = "token123", RefreshToken = "refresh123" }
            ).SetDescription("Login with valid credentials returns a token.");

            yield return new TestCaseData(
                new User { UserName = "testuser", Email = "test@example.com" },
                "WrongPassword",
                false,
                null
            ).SetDescription("Login with invalid credentials throws UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task LoginUserAsync_TestCases(User user, string password, bool isValid, AccessTokenData? expectedToken)
        {
            // Arrange
            var loginParams = new LoginUserModel(user, password);

            userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(isValid);

            if (isValid)
            {
                tokenServiceMock.Setup(x => x.CreateNewTokenDataAsync(
                    user,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedToken!);

                tokenServiceMock.Setup(x => x.SetRefreshTokenAsync(user, It.IsAny<AccessTokenData>(), CancellationToken.None))
                    .ReturnsAsync(IdentityResult.Success);
            }

            // Act & Assert
            if (isValid)
            {
                var result = await authService.LoginUserAsync(loginParams, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedToken));
            }
            else
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginUserAsync(loginParams, CancellationToken.None));
            }
        }

        [Test]
        public void LoginUserAsync_UsetRefreshTokenFails_ThrowsError()
        {
            // Arrange
            var loginParams = new LoginUserModel(new User { UserName = "testuser", Email = "test@example.com" }, "password");
            var token = new AccessTokenData { AccessToken = "token123", RefreshToken = "refresh123" };

            userManagerMock.Setup(x => x.CheckPasswordAsync(loginParams.User, loginParams.Password)).ReturnsAsync(true);

            tokenServiceMock.Setup(x => x.CreateNewTokenDataAsync(
                   loginParams.User,
                   It.IsAny<DateTime>(),
                   It.IsAny<CancellationToken>()))
                   .ReturnsAsync(token);

            tokenServiceMock.Setup(x => x.SetRefreshTokenAsync(
                  loginParams.User,
                  token,
                  It.IsAny<CancellationToken>()))
                  .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            // Act & Assert
            Assert.ThrowsAsync<AuthorizationException>(() => authService.LoginUserAsync(loginParams, CancellationToken.None));
        }

        private static IEnumerable<TestCaseData> RefreshTokenTestCases()
        {
            var validUser = new User
            {
                UserName = "testuser",
                RefreshToken = "valid-refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };

            var expiredUser = new User
            {
                UserName = "testuser",
                RefreshToken = "expired-refresh",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };

            var validTokenData = new AccessTokenData { AccessToken = "access-token", RefreshToken = "valid-refresh" };
            var invalidTokenData = new AccessTokenData { AccessToken = "invalid-access-token", RefreshToken = "invalid-refresh" };
            var newTokenData = new AccessTokenData { AccessToken = "new-token", RefreshToken = "new-refresh" };

            yield return new TestCaseData(
                validUser,
                validTokenData,
                newTokenData,
                true
            ).SetDescription("Refresh with valid token and user returns a new token.");

            yield return new TestCaseData(
                expiredUser,
                validTokenData,
                null,
                false
            ).SetDescription("Refresh with expired token throws UnauthorizedAccessException.");

            yield return new TestCaseData(
                validUser,
                invalidTokenData,
                null,
                false
            ).SetDescription("Refresh with invalid token throws UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshTokenTestCases))]
        public async Task RefreshTokenAsync_TestCases(User user, AccessTokenData tokenData, AccessTokenData? newTokenData, bool isValid)
        {
            // Arrange
            var tokenParams = new RefreshTokenModel(user, tokenData);

            if (isValid)
            {
                tokenServiceMock.Setup(x => x.CreateNewTokenDataAsync(
                    user,
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                    .ReturnsAsync(newTokenData!);

                tokenServiceMock.Setup(x => x.SetRefreshTokenAsync(user, It.IsAny<AccessTokenData>(), CancellationToken.None))
                    .ReturnsAsync(IdentityResult.Success);
            }

            // Act & Assert
            if (isValid)
            {
                var result = await authService.RefreshTokenAsync(tokenParams, CancellationToken.None);
                Assert.That(result, Is.EqualTo(newTokenData));
            }
            else
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RefreshTokenAsync(tokenParams, CancellationToken.None));
            }
        }

        [Test]
        public void RefreshTokenAsync_UsetRefreshTokenFails_ThrowsError()
        {
            // Arrange
            var token = new AccessTokenData { AccessToken = "token123", RefreshToken = "refresh123" };
            var tokenParams = new RefreshTokenModel(
                new User
                {
                    UserName = "testuser",
                    Email = "test@example.com",
                    RefreshToken = token.RefreshToken,
                    RefreshTokenExpiryTime = DateTime.MaxValue,
                },
                token);

            tokenServiceMock.Setup(x => x.CreateNewTokenDataAsync(
                   tokenParams.User,
                   It.IsAny<DateTime>(),
                   It.IsAny<CancellationToken>()))
                   .ReturnsAsync(token);

            tokenServiceMock.Setup(x => x.SetRefreshTokenAsync(
                  tokenParams.User,
                  token,
                  It.IsAny<CancellationToken>()))
                  .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

            // Act & Assert
            Assert.ThrowsAsync<AuthorizationException>(() => authService.RefreshTokenAsync(tokenParams, CancellationToken.None));
        }
    }
}
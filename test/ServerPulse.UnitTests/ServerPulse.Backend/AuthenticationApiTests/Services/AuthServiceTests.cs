using Authentication.Models;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Services.Tests
{
    [TestFixture]
    internal class AuthServiceTests
    {
        private Mock<UserManager<User>> userManagerMock;
        private Mock<ITokenService> tokenServiceMock;
        private AuthService authService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();

            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            tokenServiceMock = new Mock<ITokenService>();

            authService = new AuthService(userManagerMock.Object, tokenServiceMock.Object);
        }

        private static IEnumerable<TestCaseData> RegisterUserTestCases()
        {
            yield return new TestCaseData(
                new RegisterUserModel { User = new User { UserName = "testuser", Email = "test@example.com" }, Password = "Password123" },
                IdentityResult.Success,
                true
            ).SetDescription("Registering a user successfully returns success.");

            yield return new TestCaseData(
                new RegisterUserModel { User = new User { UserName = "testuser", Email = "test@example.com" }, Password = "WeakPassword" },
                IdentityResult.Failed(new IdentityError { Description = "Password is too weak." }),
                false
            ).SetDescription("Registering a user with invalid password returns failure.");
        }

        [Test]
        [TestCaseSource(nameof(RegisterUserTestCases))]
        public async Task RegisterUserAsync_TestCases(RegisterUserModel model, IdentityResult identityResult, bool isSuccess)
        {
            // Arrange
            userManagerMock.Setup(x => x.CreateAsync(model.User, model.Password)).ReturnsAsync(identityResult);

            // Act
            var result = await authService.RegisterUserAsync(model, CancellationToken.None);

            // Assert
            Assert.That(result.Succeeded, Is.EqualTo(isSuccess));
        }

        private static IEnumerable<TestCaseData> LoginUserTestCases()
        {
            yield return new TestCaseData(
                "testuser",
                "CorrectPassword",
                new User { UserName = "testuser", Email = "test@example.com" },
                new AccessTokenData { AccessToken = "token123", RefreshToken = "refresh123" },
                true
            ).SetDescription("Login with valid credentials returns a token.");

            yield return new TestCaseData(
                "testuser",
                "CorrectPassword",
                null,
                null,
                false
            ).SetDescription("User not found, throws UnauthorizedAccessException.");

            yield return new TestCaseData(
                "testuser",
                "WrongPassword",
                new User { UserName = "testuser", Email = "test@example.com" },
                null,
                false
          ).SetDescription("Invalid password, throws UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserTestCases))]
        public async Task LoginUserAsync_TestCases(string login, string password, User user, AccessTokenData tokenData, bool isValid)
        {
            // Arrange
            if (user != null)
            {
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
                userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(isValid);

                if (isValid)
                {
                    tokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(tokenData);
                }
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            var loginModel = new LoginUserModel { Login = login, Password = password };

            // Act & Assert
            if (isValid)
            {
                var result = await authService.LoginUserAsync(loginModel, CancellationToken.None);
                Assert.That(result, Is.EqualTo(tokenData));
            }
            else
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginUserAsync(loginModel, CancellationToken.None));
            }
        }

        private static IEnumerable<TestCaseData> RefreshTokenTestCases()
        {
            var validUser = new User
            {
                Id = "userId123",
                RefreshToken = "valid-refresh",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1)
            };

            yield return new TestCaseData(
                new AccessTokenData { AccessToken = "access-token", RefreshToken = "valid-refresh" },
                validUser,
                new AccessTokenData { AccessToken = "new-access-token", RefreshToken = "new-refresh-token" },
                true
            ).SetDescription("Refresh token with valid data returns a new token.");

            yield return new TestCaseData(
                new AccessTokenData { AccessToken = "access-token", RefreshToken = "invalid-refresh" },
                null,
                null,
                false
            ).SetDescription("User not found, throws UnauthorizedAccessException.");
        }

        [Test]
        [TestCaseSource(nameof(RefreshTokenTestCases))]
        public async Task RefreshTokenAsync_TestCases(AccessTokenData tokenData, User user, AccessTokenData? newTokenData, bool isValid)
        {
            // Arrange
            if (user != null)
            {
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

                if (isValid)
                {
                    tokenServiceMock.Setup(x => x.RefreshAccessTokenAsync(tokenData, user, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(newTokenData!);
                }
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            tokenServiceMock.Setup(x => x.GetPrincipalFromExpiredToken(tokenData.AccessToken))
                .Returns(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user?.Id ?? "")])));

            // Act & Assert
            if (isValid)
            {
                var result = await authService.RefreshTokenAsync(tokenData, CancellationToken.None);
                Assert.That(result, Is.EqualTo(newTokenData));
            }
            else
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.RefreshTokenAsync(tokenData, CancellationToken.None));
            }
        }

        [Test]
        public async Task UpdateUserAsync_UserFoundAndValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var updateModel = new UserUpdateModel
            {
                UserName = "newuser",
                Email = "newemail@example.com",
                OldPassword = "oldpassword",
                Password = "newpassword"
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)]));

            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
            userManagerMock.Setup(x => x.SetUserNameAsync(user, updateModel.UserName)).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateChangeEmailTokenAsync(user, updateModel.Email)).ReturnsAsync("emailToken");
            userManagerMock.Setup(x => x.ChangeEmailAsync(user, updateModel.Email, "emailToken")).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.ChangePasswordAsync(user, updateModel.OldPassword, updateModel.Password)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, false, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task UpdateUserAsync_UserFoundButInvalidUserNameUpdate_ReturnsError()
        {
            // Arrange
            var user = new User { UserName = "olduser", Email = "test@example.com" };
            var updateModel = new UserUpdateModel
            {
                UserName = "existinguser",
                Email = "",
                Password = "",
                OldPassword = ""
            };
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "Username already exists." } };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)]));

            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
            userManagerMock.Setup(x => x.SetUserNameAsync(user, updateModel.UserName))
                .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, false, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First().Description, Is.EqualTo("Username already exists."));
            userManagerMock.Verify(x => x.SetUserNameAsync(user, updateModel.UserName), Times.Once);
        }

        [Test]
        public async Task UpdateUserAsync_UserFoundButInvalidEmailUpdate_ReturnsErrors()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var updateModel = new UserUpdateModel
            {
                UserName = "",
                Email = "invalidemail@example.com",
                Password = "",
                OldPassword = ""
            };
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "Invalid email format" } };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)]));

            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
            userManagerMock.Setup(x => x.GenerateChangeEmailTokenAsync(user, updateModel.Email)).ReturnsAsync("emailToken");
            userManagerMock.Setup(x => x.ChangeEmailAsync(user, updateModel.Email, "emailToken")).ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, false, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First().Description, Is.EqualTo("Invalid email format"));
        }

        [Test]
        [TestCase(false, null, "newpassword", "Password update failed due to missing old password.", Description = "Old password missing, update fails.")]
        [TestCase(false, "oldpassword", "newpassword", "Password too weak", Description = "Weak password rejected during ChangePasswordAsync.")]
        [TestCase(true, null, "newpassword", "Reset token expired.", Description = "Invalid reset token during ResetPasswordAsync.")]
        public async Task UpdateUserAsync_PasswordUpdate_InvalidScenarios(
               bool resetPassword,
               string? oldPassword,
               string newPassword,
               string? expectedError)
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var updateModel = new UserUpdateModel
            {
                UserName = "",
                Email = "",
                Password = newPassword,
                OldPassword = oldPassword
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) }));

            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

            if (resetPassword)
            {
                userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("resetToken");
                userManagerMock.Setup(x => x.ResetPasswordAsync(user, "resetToken", newPassword))
                    .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedError! }));
            }
            else if (!string.IsNullOrEmpty(oldPassword))
            {
                userManagerMock.Setup(x => x.ChangePasswordAsync(user, oldPassword, newPassword))
                    .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedError! }));
            }

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, resetPassword, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First().Description, Is.EqualTo(expectedError));
        }

        [Test]
        public void UpdateUserAsync_UserNotFound_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var updateModel = new UserUpdateModel
            {
                UserName = "newuser",
                Email = "invalidemail@example.com",
                Password = "newpassword",
                OldPassword = "oldpassword"
            };
            userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                authService.UpdateUserAsync(new ClaimsPrincipal(), updateModel, false, CancellationToken.None));
        }
    }
}
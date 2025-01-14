using Authentication.Models;
using AuthenticationApi.Core.Entities;
using AuthenticationApi.Core.Models;
using Microsoft.AspNetCore.Identity;
using MockQueryable.Moq;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Application.Services.Tests
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
            var user = new User { UserName = "testuser", Email = "test@example.com", PasswordHash = "somepasswordhash" };
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
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task UpdateUserAsync_UserHasNoRegistredPassword_AddsNewPasswordSuccessfully()
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com", PasswordHash = "" };
            var newPassword = "newpassword";
            var resetToken = "resetToken";
            var updateModel = new UserUpdateModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                OldPassword = "doesntmatterwhatpasswordishere",
                Password = newPassword
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)]));

            userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(resetToken);
            userManagerMock.Setup(x => x.ResetPasswordAsync(user, "resetToken", newPassword)).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Empty);

            userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
            userManagerMock.Verify(x => x.ResetPasswordAsync(user, resetToken, newPassword), Times.Once);
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
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, CancellationToken.None);

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
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.First().Description, Is.EqualTo("Invalid email format"));
        }

        [Test]
        [TestCase(null, "newpassword", "Password update failed due to missing old password.", Description = "Old password missing, update fails.")]
        [TestCase("oldpassword", "newpassword", "Password too weak", Description = "Weak password rejected during ChangePasswordAsync.")]
        public async Task UpdateUserAsync_PasswordUpdate_InvalidScenarios(
               string? oldPassword,
               string newPassword,
               string? expectedError)
        {
            // Arrange
            var user = new User { UserName = "testuser", Email = "test@example.com", PasswordHash = "somepasswordhash" };
            var updateModel = new UserUpdateModel
            {
                UserName = "",
                Email = "",
                Password = newPassword,
                OldPassword = oldPassword
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) }));

            userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);

            if (!string.IsNullOrEmpty(oldPassword))
            {
                userManagerMock.Setup(x => x.ChangePasswordAsync(user, oldPassword, newPassword))
                    .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = expectedError! }));
            }

            // Act
            var result = await authService.UpdateUserAsync(claimsPrincipal, updateModel, CancellationToken.None);

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
                authService.UpdateUserAsync(new ClaimsPrincipal(), updateModel, CancellationToken.None));
        }

        private static IEnumerable<TestCaseData> LoginUserWithProviderTestCases()
        {
            yield return new TestCaseData(
                new ProviderLoginModel
                {
                    Email = "existinguser@example.com",
                    ProviderLogin = "Google",
                    ProviderKey = "provider-key"
                },
                new User { Email = "existinguser@example.com" },
                true,
                true,
                new AccessTokenData { AccessToken = "access-token", RefreshToken = "refresh-token" }
            ).SetDescription("Existing user found by login returns token.");

            yield return new TestCaseData(
                new ProviderLoginModel
                {
                    Email = "newuser@example.com",
                    ProviderLogin = "Google",
                    ProviderKey = "new-provider-key"
                },
                null,
                false,
                true,
                new AccessTokenData { AccessToken = "new-access-token", RefreshToken = "new-refresh-token" }
            ).SetDescription("New user created and login added successfully returns token.");

            yield return new TestCaseData(
                new ProviderLoginModel
                {
                    Email = "newuser@example.com",
                    ProviderLogin = "Google",
                    ProviderKey = "new-provider-key"
                },
                null,
                false,
                false,
                null
            ).SetDescription("Adding login fails throws InvalidOperationException.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserWithProviderTestCases))]
        public async Task LoginUserWithProviderAsync_TestCases(
            ProviderLoginModel model,
            User? user,
            bool userFoundByLogin,
            bool addLoginSuccess,
            AccessTokenData? expectedToken)
        {
            // Arrange
            if (userFoundByLogin)
            {
                userManagerMock.Setup(x => x.FindByLoginAsync(model.ProviderLogin, model.ProviderKey))
                    .ReturnsAsync(user);

                if (user != null)
                {
                    tokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedToken!);
                }
            }
            else
            {
                userManagerMock.Setup(x => x.FindByLoginAsync(model.ProviderLogin, model.ProviderKey))
                    .ReturnsAsync((User?)null);

                userManagerMock.Setup(x => x.FindByEmailAsync(model.Email))
                    .ReturnsAsync(user);

                if (user == null)
                {
                    var newUser = new User { Email = model.Email, UserName = model.Email, EmailConfirmed = true };
                    userManagerMock.Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == newUser.Email)))
                        .ReturnsAsync(IdentityResult.Success);

                    userManagerMock.Setup(x => x.AddLoginAsync(It.Is<User>(u => u.Email == newUser.Email),
                            It.Is<UserLoginInfo>(info => info.LoginProvider == model.ProviderLogin && info.ProviderKey == model.ProviderKey)))
                        .ReturnsAsync(addLoginSuccess ? IdentityResult.Success : IdentityResult.Failed());
                }

                if (addLoginSuccess)
                {
                    tokenServiceMock.Setup(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedToken!);
                }
            }

            // Act & Assert
            if (!addLoginSuccess)
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await authService.LoginUserWithProviderAsync(model, CancellationToken.None));
            }
            else
            {
                var result = await authService.LoginUserWithProviderAsync(model, CancellationToken.None);
                Assert.That(result, Is.EqualTo(expectedToken));
            }

            userManagerMock.Verify(x => x.FindByLoginAsync(model.ProviderLogin, model.ProviderKey), Times.Once);

            if (!userFoundByLogin)
            {
                userManagerMock.Verify(x => x.FindByEmailAsync(model.Email), Times.Once);

                if (user == null)
                {
                    userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == model.Email)), Times.Once);
                    userManagerMock.Verify(x => x.AddLoginAsync(It.IsAny<User>(), It.IsAny<UserLoginInfo>()), Times.Once);
                }
            }

            if (expectedToken != null)
            {
                tokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        private static IEnumerable<TestCaseData> GetEmailConfirmationTokenTestCases()
        {
            yield return new TestCaseData(
                "validuser@example.com",
                "valid_token",
                true
            ).SetDescription("Generate email confirmation token for valid user.");

            yield return new TestCaseData(
                "invaliduser@example.com",
                null,
                false
            ).SetDescription("Generate email confirmation token fails for non-existent user.");
        }

        [Test]
        [TestCaseSource(nameof(GetEmailConfirmationTokenTestCases))]
        public async Task GetEmailConfirmationTokenAsync_TestCases(string email, string token, bool isValid)
        {
            // Arrange
            if (isValid)
            {
                var user = new User { Email = email };
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            if (isValid)
            {
                userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                    .ReturnsAsync(token!);
            }

            // Act & Assert
            if (!isValid)
            {
                Assert.ThrowsAsync<InvalidOperationException>(() => authService.GetEmailConfirmationTokenAsync(email));
            }
            else
            {
                var result = await authService.GetEmailConfirmationTokenAsync(email);
                Assert.That(result, Is.EqualTo(token));
            }
        }

        private static IEnumerable<TestCaseData> ConfirmEmailTestCases()
        {
            yield return new TestCaseData(
                "validuser@example.com",
                "valid_token",
                IdentityResult.Success,
                true
            ).SetDescription("Confirm email for valid user.");

            yield return new TestCaseData(
                "validuser@example.com",
                "invalid_token",
                IdentityResult.Failed(new IdentityError { Description = "Invalid token" }),
                false
            ).SetDescription("Confirm email fails with invalid token.");

            yield return new TestCaseData(
                "invaliduser@example.com",
                "valid_token",
                null,
                false
            ).SetDescription("Confirm email fails for non-existent user.");
        }

        [Test]
        [TestCaseSource(nameof(ConfirmEmailTestCases))]
        public async Task ConfirmEmailAsync_TestCases(string email, string token, IdentityResult result, bool isValid)
        {
            // Arrange
            var user = isValid && result != null ? new User { Email = email } : null;

            if (user != null)
            {
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
                userManagerMock.Setup(x => x.ConfirmEmailAsync(user, token)).ReturnsAsync(result!);
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            // Act & Assert
            if (!isValid || result != IdentityResult.Success)
            {
                Assert.ThrowsAsync<InvalidOperationException>(() => authService.ConfirmEmailAsync(email, token));
            }
            else
            {
                var confirmResult = await authService.ConfirmEmailAsync(email, token);
                Assert.That(confirmResult, Is.EqualTo(result));
            }
        }

        private static IEnumerable<TestCaseData> LoginUserAfterConfirmationTestCases()
        {
            yield return new TestCaseData(
                "validuser@example.com",
                true,
                new AccessTokenData { AccessToken = "token123", RefreshToken = "refresh123" },
                true
            ).SetDescription("Login user after email confirmation.");

            yield return new TestCaseData(
                "validuser@example.com",
                false,
                null,
                true
            ).SetDescription("Login user fails when email is not confirmed.");

            yield return new TestCaseData(
                "invaliduser@example.com",
                false,
                null,
                false
            ).SetDescription("Login user fails for non-existent user.");
        }

        [Test]
        [TestCaseSource(nameof(LoginUserAfterConfirmationTestCases))]
        public async Task LoginUserAfterConfirmationAsync_TestCases(string email, bool isEmailConfirmed, AccessTokenData? tokenData, bool isValid)
        {
            // Arrange
            var user = isValid ? new User { Email = email } : null;

            if (user != null)
            {
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
                userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(isEmailConfirmed);
                if (isValid && isEmailConfirmed)
                {
                    tokenServiceMock.Setup(x => x.GenerateTokenAsync(user, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(tokenData!);
                }
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            // Act & Assert
            if (!isValid || !isEmailConfirmed)
            {
                Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginUserAfterConfirmationAsync(email, CancellationToken.None));
            }
            else
            {
                var result = await authService.LoginUserAfterConfirmationAsync(email, CancellationToken.None);
                Assert.That(result, Is.EqualTo(tokenData));
            }
        }

        private static IEnumerable<TestCaseData> CheckEmailConfirmationTestCases()
        {
            yield return new TestCaseData(
                "validuser@example.com",
                true,
                true
            ).SetDescription("Check email confirmation returns true for confirmed email.");

            yield return new TestCaseData(
                "validuser@example.com",
                false,
                false
            ).SetDescription("Check email confirmation returns false for unconfirmed email.");

            yield return new TestCaseData(
                "invaliduser@example.com",
                false,
                false
            ).SetDescription("Check email confirmation returns false for non-existent user.");
        }

        [Test]
        [TestCaseSource(nameof(CheckEmailConfirmationTestCases))]
        public async Task CheckEmailConfirmationAsync_TestCases(string email, bool isEmailConfirmed, bool expectedResult)
        {
            // Arrange
            var user = expectedResult || isEmailConfirmed ? new User { Email = email } : null;

            if (user != null)
            {
                userManagerMock.Setup(x => x.Users).Returns(new[] { user }.AsQueryable().BuildMockDbSet().Object);
                userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(isEmailConfirmed);
            }
            else
            {
                userManagerMock.Setup(x => x.Users).Returns(new User[0].AsQueryable().BuildMockDbSet().Object);
            }

            // Act
            var result = await authService.CheckEmailConfirmationAsync(email);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
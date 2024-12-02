using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Services.Tests
{
    [TestFixture]
    internal class UserServiceTests
    {
        private Mock<UserManager<User>> userManagerMock;
        private UserService userService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            userService = new UserService(userManagerMock.Object);
        }

        private static IEnumerable<TestCaseData> GetUserTestCases()
        {
            yield return new TestCaseData(
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user-id") })),
                new User { Id = "user-id", UserName = "testuser" }
            ).SetDescription("Valid ClaimsPrincipal with matching user should return user.");

            yield return new TestCaseData(
                new ClaimsPrincipal(new ClaimsIdentity()),
                null
            ).SetDescription("ClaimsPrincipal with no identifier should return null.");

            yield return new TestCaseData(
                new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "unknown-id") })),
                null
            ).SetDescription("ClaimsPrincipal with unknown identifier should return null.");
        }

        [Test]
        [TestCaseSource(nameof(GetUserTestCases))]
        public async Task GetUserAsync_TestCases(ClaimsPrincipal principal, User? expectedUser)
        {
            // Arrange
            if (expectedUser != null)
            {
                userManagerMock.Setup(x => x.FindByIdAsync(expectedUser.Id)).ReturnsAsync(expectedUser);
            }

            // Act
            var result = await userService.GetUserAsync(principal, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedUser));
        }

        private static IEnumerable<TestCaseData> GetUserByLoginTestCases()
        {
            var user = new User { Id = "user-id", UserName = "testuser", Email = "test@example.com" };

            yield return new TestCaseData(
                "test@example.com",
                user
            ).SetDescription("Login matches user email and should return the user.");

            yield return new TestCaseData(
                "testuser",
                user
            ).SetDescription("Login matches user username and should return the user.");

            yield return new TestCaseData(
                "user-id",
                user
            ).SetDescription("Login matches user ID and should return the user.");

            yield return new TestCaseData(
                "unknown-login",
                null
            ).SetDescription("Login does not match any user and should return null.");
        }

        [Test]
        [TestCaseSource(nameof(GetUserByLoginTestCases))]
        public async Task GetUserByLoginAsync_TestCases(string login, User? expectedUser)
        {
            // Arrange
            if (expectedUser != null)
            {
                userManagerMock.Setup(x => x.FindByEmailAsync(login)).ReturnsAsync(login == expectedUser.Email ? expectedUser : null);
                userManagerMock.Setup(x => x.FindByNameAsync(login)).ReturnsAsync(login == expectedUser.UserName ? expectedUser : null);
                userManagerMock.Setup(x => x.FindByIdAsync(login)).ReturnsAsync(login == expectedUser.Id ? expectedUser : null);
            }

            // Act
            var result = await userService.GetUserByLoginAsync(login, CancellationToken.None);

            // Assert
            Assert.That(result, Is.EqualTo(expectedUser));
        }

        private static IEnumerable<TestCaseData> UpdateUserTestCases()
        {
            var user = new User { UserName = "testuser", Email = "test@example.com" };
            var validUpdate = new UserUpdateModel
            {
                UserName = "newuser",
                OldEmail = "test@example.com",
                NewEmail = "newemail@example.com",
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };

            var invalidUpdate = new UserUpdateModel
            {
                UserName = "newuser",
                OldEmail = "test@example.com",
                NewEmail = "newemail@example.com",
                OldPassword = "oldpass",
                NewPassword = "weakpass"
            };

            yield return new TestCaseData(
                user,
                validUpdate,
                true,
                false
            ).SetDescription("Valid update should result in no errors.");

            yield return new TestCaseData(
              user,
              validUpdate,
              true,
              true
            ).SetDescription("Valid update with reset password should result in no errors.");

            yield return new TestCaseData(
                user,
                invalidUpdate,
                false,
                false
            ).SetDescription("Invalid update should result in errors.");
        }

        [Test]
        [TestCaseSource(nameof(UpdateUserTestCases))]
        public async Task UpdateUserAsync_TestCases(User user, UserUpdateModel updateModel, bool isValid, bool resetPassword)
        {
            // Arrange
            if (isValid)
            {
                userManagerMock.Setup(x => x.SetUserNameAsync(user, updateModel.UserName)).ReturnsAsync(IdentityResult.Success);
                userManagerMock.Setup(x => x.GenerateChangeEmailTokenAsync(user, updateModel.NewEmail!)).ReturnsAsync("emailToken");
                userManagerMock.Setup(x => x.ChangeEmailAsync(user, updateModel.NewEmail!, "emailToken")).ReturnsAsync(IdentityResult.Success);

                userManagerMock.Setup(x => x.ChangePasswordAsync(user, updateModel.OldPassword, updateModel.NewPassword!))
                    .ReturnsAsync(IdentityResult.Success);

                userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                    .ReturnsAsync("passwordResetToken");
                userManagerMock.Setup(x => x.ResetPasswordAsync(user, "passwordResetToken", updateModel.NewPassword!))
                    .ReturnsAsync(IdentityResult.Success);
            }
            else
            {
                var identityErrors = new List<IdentityError> { new IdentityError { Description = "Password too weak" } };

                userManagerMock.Setup(x => x.SetUserNameAsync(user, updateModel.UserName)).ReturnsAsync(IdentityResult.Success);
                userManagerMock.Setup(x => x.GenerateChangeEmailTokenAsync(user, updateModel.NewEmail!)).ReturnsAsync("emailToken");
                userManagerMock.Setup(x => x.ChangeEmailAsync(user, updateModel.NewEmail!, "emailToken")).ReturnsAsync(IdentityResult.Success);

                userManagerMock.Setup(x => x.ChangePasswordAsync(user, updateModel.OldPassword, updateModel.NewPassword!))
                    .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

                userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
                   .ReturnsAsync("passwordResetToken");
                userManagerMock.Setup(x => x.ResetPasswordAsync(user, "passwordResetToken", updateModel.NewPassword!))
                   .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));
            }

            // Act
            var result = await userService.UpdateUserAsync(user, updateModel, resetPassword, CancellationToken.None);

            // Assert
            if (isValid)
            {
                Assert.That(result, Is.Empty);
            }
            else
            {
                Assert.That(result, Is.Not.Empty);
            }
        }

        [Test]
        public async Task CheckPasswordAsync_ValidPassword_ReturnsTrue()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            const string password = "Password123";

            userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(true);

            // Act
            var result = await userService.CheckPasswordAsync(user, password, CancellationToken.None);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckPasswordAsync_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            var user = new User { UserName = "testuser" };
            const string password = "WrongPassword";

            userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(false);

            // Act
            var result = await userService.CheckPasswordAsync(user, password, CancellationToken.None);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
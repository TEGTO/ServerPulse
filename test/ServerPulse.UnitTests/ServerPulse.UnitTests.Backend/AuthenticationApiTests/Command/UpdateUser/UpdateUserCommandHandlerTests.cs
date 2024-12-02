using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Command.Tests
{
    [TestFixture]
    internal class UpdateUserCommandHandlerTests
    {
        private Mock<IUserService> mockUserService;
        private Mock<IMapper> mockMapper;
        private UpdateUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockUserService = new Mock<IUserService>();
            mockMapper = new Mock<IMapper>();
            handler = new UpdateUserCommandHandler(mockUserService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> UpdateUserTestCases()
        {
            var validPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "valid-user-id") }));
            var validRequest = new UserUpdateDataRequest
            {
                UserName = "newuser",
                OldEmail = "old@example.com",
                NewEmail = "new@example.com",
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };

            var validUpdateModel = new UserUpdateModel
            {
                UserName = "newuser",
                OldEmail = "old@example.com",
                NewEmail = "new@example.com",
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };

            var validUser = new User { Id = "valid-user-id", UserName = "olduser", Email = "old@example.com" };

            yield return new TestCaseData(
                new UpdateUserCommand(validRequest, validPrincipal),
                validUpdateModel,
                validUser,
                new List<IdentityError>(),
                true
            ).SetDescription("Valid update should succeed without errors.");

            var invalidPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "invalid-user-id") }));
            var invalidRequest = new UserUpdateDataRequest
            {
                UserName = "invaliduser",
                OldEmail = "invalid@example.com",
                NewEmail = "invalidnew@example.com",
                OldPassword = "wrongpass",
                NewPassword = "invalidnewpass"
            };

            yield return new TestCaseData(
                new UpdateUserCommand(invalidRequest, invalidPrincipal),
                new UserUpdateModel()
                {
                    UserName = "notfounduser",
                    OldEmail = "old@example.com",
                    NewEmail = "new@example.com",
                    OldPassword = "oldpass",
                    NewPassword = "newpass"
                },
                null,
                null,
                false
            ).SetDescription("User not found should throw InvalidOperationException.");

            yield return new TestCaseData(
                new UpdateUserCommand(validRequest, validPrincipal),
                validUpdateModel,
                validUser,
                new List<IdentityError> { new IdentityError { Description = "Error updating user." } },
                false
            ).SetDescription("Errors during update should throw AuthorizationException.");
        }

        [Test]
        [TestCaseSource(nameof(UpdateUserTestCases))]
        public async Task Handle_UpdateUserCommand_TestCases(
            UpdateUserCommand command,
            UserUpdateModel updateModel,
            User? user,
            List<IdentityError>? identityErrors,
            bool isValid)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<UserUpdateModel>(command.Request)).Returns(updateModel);

            if (user != null)
            {
                mockUserService.Setup(m => m.GetUserAsync(command.UserPrincipal, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(user);

                mockUserService.Setup(m => m.UpdateUserAsync(user, updateModel, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(identityErrors ?? new List<IdentityError>());
            }
            else
            {
                mockUserService.Setup(m => m.GetUserAsync(command.UserPrincipal, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);
            }

            // Act & Assert
            if (!isValid)
            {
                if (user == null)
                {
                    Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
                }
                else
                {
                    Assert.ThrowsAsync<AuthorizationException>(() => handler.Handle(command, CancellationToken.None));
                }
            }
            else
            {
                var result = await handler.Handle(command, CancellationToken.None);

                Assert.That(result, Is.EqualTo(Unit.Value));
                mockUserService.Verify(m => m.GetUserAsync(command.UserPrincipal, It.IsAny<CancellationToken>()), Times.Once);
                mockUserService.Verify(m => m.UpdateUserAsync(user!, updateModel, false, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure.Models;
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
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private UpdateUserCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            handler = new UpdateUserCommandHandler(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> UpdateUserTestCases()
        {
            var validPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "valid-user-id") }));
            var validRequest = new UserUpdateDataRequest
            {
                Email = "new@example.com",
                OldPassword = "oldpass",
                Password = "newpass"
            };

            var validUpdateModel = new UserUpdateModel
            {
                UserName = "newuser",
                Email = "new@example.com",
                OldPassword = "oldpass",
                Password = "newpass"
            };

            yield return new TestCaseData(
                new UpdateUserCommand(validRequest, validPrincipal),
                validUpdateModel,
                new List<IdentityError>(),
                true
            ).SetDescription("Valid update should succeed without errors.");

            yield return new TestCaseData(
                new UpdateUserCommand(validRequest, validPrincipal),
                validUpdateModel,
                new List<IdentityError> { new IdentityError { Description = "Error updating user." } },
                false
            ).SetDescription("Errors during update should throw AuthorizationException.");
        }

        [Test]
        [TestCaseSource(nameof(UpdateUserTestCases))]
        public async Task Handle_UpdateUserCommand_TestCases(
            UpdateUserCommand command,
            UserUpdateModel updateModel,
            List<IdentityError>? identityErrors,
            bool isValid)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<UserUpdateModel>(command.Request)).Returns(updateModel);

            mockAuthService.Setup(m => m.UpdateUserAsync(command.UserPrincipal, updateModel, false, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(identityErrors ?? new List<IdentityError>());

            // Act & Assert
            if (!isValid)
            {
                Assert.ThrowsAsync<AuthorizationException>(() => handler.Handle(command, CancellationToken.None));
            }
            else
            {
                var result = await handler.Handle(command, CancellationToken.None);

                Assert.That(result, Is.EqualTo(Unit.Value));
                mockAuthService.Verify(m => m.UpdateUserAsync(command.UserPrincipal, updateModel, false, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
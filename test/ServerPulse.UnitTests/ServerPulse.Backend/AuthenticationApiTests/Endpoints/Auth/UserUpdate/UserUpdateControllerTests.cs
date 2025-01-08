using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.UserUpdate;
using AuthenticationApi.Infrastructure.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AuthenticationApi.Endpoints.Auth.UserUpdate.Tests
{
    [TestFixture]
    internal class UserUpdateControllerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private UserUpdateController controller;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            controller = new UserUpdateController(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> UpdateUserTestCases()
        {
            var validRequest = new UserUpdateRequest
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
                validRequest,
                validUpdateModel,
                new List<IdentityError>(),
                true
            ).SetDescription("Valid update should succeed without errors, response Ok.");

            yield return new TestCaseData(
                validRequest,
                validUpdateModel,
                new List<IdentityError> { new IdentityError { Description = "Error updating user." } },
                false
            ).SetDescription("Errors during update should response Unauthorized.");
        }

        [Test]
        [TestCaseSource(nameof(UpdateUserTestCases))]
        public async Task UserUpdate_TestCases(
            UserUpdateRequest request,
            UserUpdateModel updateModel,
            List<IdentityError>? identityErrors,
            bool isValid)
        {
            // Arrange
            mockMapper.Setup(m => m.Map<UserUpdateModel>(request)).Returns(updateModel);

            mockAuthService.Setup(m => m.UpdateUserAsync(It.IsAny<ClaimsPrincipal>(), updateModel, false, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(identityErrors ?? new List<IdentityError>());

            if (!isValid)
            {
                // Act
                var result = await controller.UserUpdate(request, CancellationToken.None);

                // Assert
                Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
            }
            else
            {
                // Act
                var result = await controller.UserUpdate(request, CancellationToken.None);

                // Assert
                Assert.That(result, Is.InstanceOf<OkResult>());

                mockAuthService.Verify(m => m.UpdateUserAsync(It.IsAny<ClaimsPrincipal>(), updateModel, false, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}
using AuthenticationApi.Command;
using AuthenticationApi.Command.LoginUser;
using AuthenticationApi.Command.RefreshToken;
using AuthenticationApi.Command.RegisterUser;
using AuthenticationApi.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Controllers.Tests
{
    [TestFixture]
    internal class AuthControllerTests
    {
        private Mock<IMediator> mediatorMock;
        private AuthController authController;

        [SetUp]
        public void SetUp()
        {
            mediatorMock = new Mock<IMediator>();

            authController = new AuthController(mediatorMock.Object);
        }

        [Test]
        public async Task Register_SendsCommandAndReturnsAuthResponse()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest { Email = "testuser@example.com", Password = "Password123", ConfirmPassword = "Password123" };
            var userAuthResponse = new UserAuthenticationResponse { Email = "testuser@example.com" };

            mediatorMock.Setup(m => m.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userAuthResponse);

            // Act
            var result = await authController.Register(registrationRequest, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);

            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdAtActionResult);

            Assert.IsNotNull(createdAtActionResult.RouteValues);
            Assert.That(createdAtActionResult.RouteValues["id"], Is.EqualTo(userAuthResponse.Email));

            mediatorMock.Verify(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Login_SendsCommandAndReturnsAuthResponse()
        {
            // Arrange
            var loginRequest = new UserAuthenticationRequest { Login = "testuser", Password = "Password123" };
            var userAuthResponse = new UserAuthenticationResponse { Email = "testuser@example.com" };

            mediatorMock.Setup(m => m.Send(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(userAuthResponse);

            // Act
            var result = await authController.Login(loginRequest, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult.Value, Is.EqualTo(userAuthResponse));

            mediatorMock.Verify(x => x.Send(It.IsAny<LoginUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Update_SendsCommandAndReturnsOk()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest { Email = "newemail@example.com", OldPassword = "oldpass", Password = "newpass" };

            // Act
            var result = await authController.Update(updateRequest, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);

            mediatorMock.Verify(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Refresh_SendsCommandAndReturnsAuthToken()
        {
            // Arrange
            var token = new AccessTokenDataDto { AccessToken = "token", RefreshToken = "refreshToken" };
            var refreshedToken = new AccessTokenDataDto { AccessToken = "newToken", RefreshToken = "newRefreshToken" };

            mediatorMock.Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshedToken);

            // Act
            var result = await authController.Refresh(token, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult?.Value, Is.EqualTo(refreshedToken));

            mediatorMock.Verify(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
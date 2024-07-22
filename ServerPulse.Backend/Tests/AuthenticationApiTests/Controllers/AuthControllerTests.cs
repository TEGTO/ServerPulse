using Authentication.Models;
using AuthenticationApi.Controllers;
using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;
using AuthenticationApi.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.Dtos;
using Shared.Dtos.Auth;
using System.Net;

namespace AuthenticationApiTests.Controllers
{
    [TestFixture]
    internal class AuthControllerTests
    {
        private const int EXPIRY_IN_DAYS = 7;

        private Mock<IMapper> mapperMock;
        private Mock<IAuthService> authServiceMock;
        private Mock<IConfiguration> configurationMock;
        private AuthController authController;

        [SetUp]
        public void SetUp()
        {
            mapperMock = new Mock<IMapper>();
            authServiceMock = new Mock<IAuthService>();
            configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(config => config["AuthSettings:RefreshExpiryInDays"]).Returns(EXPIRY_IN_DAYS.ToString());
            authController = new AuthController(mapperMock.Object, authServiceMock.Object, configurationMock.Object);
        }

        [Test]
        public async Task Register_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest { UserName = "testuser", Email = "testuser@example.com", Password = "Password123", ConfirmPassword = "Password123" };
            var user = new User { Id = "1", UserName = "testuser", Email = "testuser@example.com" };
            var identityResult = IdentityResult.Success;
            mapperMock.Setup(m => m.Map<User>(registrationRequest)).Returns(user);
            authServiceMock.Setup(a => a.RegisterUserAsync(user, registrationRequest.Password)).ReturnsAsync(identityResult);

            // Act
            var result = await authController.Register(registrationRequest);

            // Assert
            Assert.IsInstanceOf<CreatedResult>(result);
            var createdResult = result as CreatedResult;
            Assert.That(createdResult.Location, Is.EqualTo($"/users/{user.Id}"));
        }
        [Test]
        public async Task Register_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            UserRegistrationRequest registrationRequest = null;

            // Act
            var result = await authController.Register(registrationRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid client request"));
        }
        [Test]
        public async Task Register_FailedRegistration_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var registrationRequest = new UserRegistrationRequest { UserName = "testuser", Email = "testuser@example.com", Password = "Password123", ConfirmPassword = "Password123" };
            var user = new User { Id = "1", UserName = "testuser", Email = "testuser@example.com" };
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "Error" });
            mapperMock.Setup(m => m.Map<User>(registrationRequest)).Returns(user);
            authServiceMock.Setup(a => a.RegisterUserAsync(user, registrationRequest.Password)).ReturnsAsync(identityResult);

            // Act
            var result = await authController.Register(registrationRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var responseError = badRequestResult.Value as ResponseError;
            Assert.That(responseError.StatusCode, Is.EqualTo(((int)HttpStatusCode.BadRequest).ToString()));
            Assert.That(responseError.Messages[0], Is.EqualTo("Error"));
        }
        [Test]
        public async Task Login_ValidRequest_ReturnsOk()
        {
            // Arrange
            var authRequest = new UserAuthenticationRequest { Login = "testuser", Password = "Password123" };
            var tokenData = new AccessTokenData { AccessToken = "token", RefreshToken = "refreshToken" };
            var tokenDto = new AuthToken { AccessToken = "token", RefreshToken = "refreshToken", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(EXPIRY_IN_DAYS) };
            var user = new User { UserName = "testuser", Email = "testuser@example.com" };
            authServiceMock.Setup(a => a.LoginUserAsync(authRequest.Login, authRequest.Password, EXPIRY_IN_DAYS)).ReturnsAsync(tokenData);
            mapperMock.Setup(m => m.Map<AuthToken>(tokenData)).Returns(tokenDto);
            authServiceMock.Setup(a => a.GetUserByLoginAsync(authRequest.Login)).ReturnsAsync(user);

            // Act
            var result = await authController.Login(authRequest);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as UserAuthenticationResponse;
            Assert.That(response.AuthToken, Is.EqualTo(tokenDto));
            Assert.That(response.UserName, Is.EqualTo(user.UserName));
            Assert.That(response.Email, Is.EqualTo(user.Email));
        }
        [Test]
        public async Task Update_ValidRequest_ReturnsOk()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest { UserName = "newuser", OldEmail = "old@example.com", NewEmail = "new@example.com", OldPassword = "OldPassword123", NewPassword = "NewPassword123" };
            var serviceUpdateRequest = new UserUpdateData { UserName = "newuser", OldEmail = "old@example.com", NewEmail = "new@example.com", OldPassword = "OldPassword123", NewPassword = "NewPassword123" };
            mapperMock.Setup(m => m.Map<UserUpdateData>(updateRequest)).Returns(serviceUpdateRequest);
            authServiceMock.Setup(a => a.UpdateUserAsync(serviceUpdateRequest)).ReturnsAsync(new List<IdentityError>());

            // Act
            var result = await authController.Update(updateRequest);

            // Assert
            Assert.IsInstanceOf<OkResult>(result);
        }
        [Test]
        public async Task Update_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange
            var updateRequest = new UserUpdateDataRequest { UserName = "newuser", OldEmail = "old@example.com", NewEmail = "new@example.com", OldPassword = "OldPassword123", NewPassword = "NewPassword123" };
            var serviceUpdateRequest = new UserUpdateData { UserName = "newuser", OldEmail = "old@example.com", NewEmail = "new@example.com", OldPassword = "OldPassword123", NewPassword = "NewPassword123" };
            var identityErrors = new List<IdentityError> { new IdentityError { Description = "Error" } };
            mapperMock.Setup(m => m.Map<UserUpdateData>(updateRequest)).Returns(serviceUpdateRequest);
            authServiceMock.Setup(a => a.UpdateUserAsync(serviceUpdateRequest)).ReturnsAsync(identityErrors);

            // Act
            var result = await authController.Update(updateRequest);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var responseError = badRequestResult.Value as ResponseError;
            Assert.That(responseError.StatusCode, Is.EqualTo(((int)HttpStatusCode.BadRequest).ToString()));
            Assert.That(responseError.Messages[0], Is.EqualTo("Error"));
        }
        [Test]
        public async Task Refresh_ValidRequest_ReturnsOk()
        {
            // Arrange
            var accessTokenDto = new AuthToken { AccessToken = "token", RefreshToken = "refreshToken" };
            var accessTokenData = new AccessTokenData { AccessToken = "token", RefreshToken = "refreshToken" };
            var newTokenData = new AccessTokenData { AccessToken = "newToken", RefreshToken = "newRefreshToken" };
            var newTokenDto = new AuthToken { AccessToken = "newToken", RefreshToken = "newRefreshToken", RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(EXPIRY_IN_DAYS) };
            mapperMock.Setup(m => m.Map<AccessTokenData>(accessTokenDto)).Returns(accessTokenData);
            authServiceMock.Setup(a => a.RefreshTokenAsync(accessTokenData, EXPIRY_IN_DAYS)).ReturnsAsync(newTokenData);
            mapperMock.Setup(m => m.Map<AuthToken>(newTokenData)).Returns(newTokenDto);

            // Act
            var result = await authController.Refresh(accessTokenDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as AuthToken;
            Assert.That(response.AccessToken, Is.EqualTo(newTokenDto.AccessToken));
            Assert.That(response.RefreshToken, Is.EqualTo(newTokenDto.RefreshToken));
            Assert.That(response.RefreshTokenExpiryDate, Is.EqualTo(newTokenDto.RefreshTokenExpiryDate));
        }
        [Test]
        public async Task CheckAuthData_ValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Login = "testuser", Password = "Password123" };
            var checkAuthDataResponse = new CheckAuthDataResponse { IsCorrect = true };
            authServiceMock.Setup(a => a.CheckAuthDataAsync(request.Login, request.Password)).ReturnsAsync(true);
            // Act
            var result = await authController.CheckAuthData(request);
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var response = okResult.Value as CheckAuthDataResponse;
            Assert.That(response.IsCorrect, Is.EqualTo(checkAuthDataResponse.IsCorrect));
        }
    }
}
using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Services;
using AutoMapper;
using ExceptionHandling;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace AuthenticationApi.Command.ConfirmEmail.Tests
{
    [TestFixture]
    internal class ConfirmEmailCommandHandlerTests
    {
        private Mock<IAuthService> mockAuthService;
        private Mock<IMapper> mockMapper;
        private ConfirmEmailCommandHandler handler;

        [SetUp]
        public void SetUp()
        {
            mockAuthService = new Mock<IAuthService>();
            mockMapper = new Mock<IMapper>();

            handler = new ConfirmEmailCommandHandler(mockAuthService.Object, mockMapper.Object);
        }

        private static IEnumerable<TestCaseData> ConfirmEmailTestCases()
        {
            var validRequest = new EmailConfirmationRequest
            {
                Email = "validuser@example.com",
                Token = "valid_token"
            };

            var validAccessTokenData = new AccessTokenData
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(60)
            };

            var validAccessTokenDto = new AccessTokenDataDto
            {
                AccessToken = validAccessTokenData.AccessToken,
                RefreshToken = validAccessTokenData.RefreshToken,
                RefreshTokenExpiryDate = validAccessTokenData.RefreshTokenExpiryDate
            };

            var validResponse = new UserAuthenticationResponse
            {
                AuthToken = validAccessTokenDto,
                Email = validRequest.Email
            };

            yield return new TestCaseData(
                new ConfirmEmailCommand(validRequest),
                IdentityResult.Success,
                validAccessTokenData,
                validAccessTokenDto,
                validResponse,
                true
            ).SetDescription("Valid email confirmation and login should return authentication response.");

            yield return new TestCaseData(
                new ConfirmEmailCommand(validRequest),
                IdentityResult.Failed(new IdentityError { Description = "Invalid token" }),
                null,
                null,
                null,
                false
            ).SetDescription("Invalid email confirmation token should throw AuthorizationException.");
        }

        [Test]
        [TestCaseSource(nameof(ConfirmEmailTestCases))]
        public async Task Handle_ConfirmEmailCommand_TestCases(
            ConfirmEmailCommand command,
            IdentityResult confirmResult,
            AccessTokenData? accessTokenData,
            AccessTokenDataDto? accessTokenDto,
            UserAuthenticationResponse? expectedResponse,
            bool isValid)
        {
            // Arrange
            mockAuthService.Setup(x => x.ConfirmEmailAsync(command.Request.Email, command.Request.Token))
                .ReturnsAsync(confirmResult);

            if (isValid)
            {
                mockAuthService.Setup(x => x.LoginUserAfterConfirmationAsync(command.Request.Email, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(accessTokenData!);

                mockMapper.Setup(x => x.Map<AccessTokenDataDto>(accessTokenData!))
                    .Returns(accessTokenDto!);
            }

            // Act & Assert
            if (!isValid)
            {
                Assert.ThrowsAsync<AuthorizationException>(() => handler.Handle(command, CancellationToken.None));
            }
            else
            {
                var result = await handler.Handle(command, CancellationToken.None);

                Assert.IsNotNull(result);
                Assert.That(result.AuthToken, Is.EqualTo(expectedResponse?.AuthToken));
                Assert.That(result.Email, Is.EqualTo(expectedResponse?.Email));

                mockAuthService.Verify(x => x.ConfirmEmailAsync(command.Request.Email, command.Request.Token), Times.Once);
                mockAuthService.Verify(x => x.LoginUserAfterConfirmationAsync(command.Request.Email, It.IsAny<CancellationToken>()), Times.Once);
                mockMapper.Verify(x => x.Map<AccessTokenDataDto>(It.IsAny<AccessTokenData>()), Times.Once);
            }
        }
    }
}
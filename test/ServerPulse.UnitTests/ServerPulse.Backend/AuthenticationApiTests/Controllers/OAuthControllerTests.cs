using AuthenticationApi.Command.GetOAuthUrlQuery;
using AuthenticationApi.Command.LoginOAuthCommand;
using AuthenticationApi.Dtos;
using AuthenticationApi.Dtos.OAuth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace AuthenticationApi.Controllers.Tests
{
    [TestFixture]
    internal class OAuthControllerTests
    {
        private Mock<IMediator> mockMediator;
        private OAuthController controller;

        [SetUp]
        public void SetUp()
        {
            mockMediator = new Mock<IMediator>();

            controller = new OAuthController(mockMediator.Object);
        }

        [Test]
        public async Task GetOAuthUrl_ValidQueryParams_ReturnsOkWithResponse()
        {
            // Arrange
            var queryParams = new GetOAuthUrlQueryParams();
            var response = new GetOAuthUrlResponse { Url = "someurl" };

            mockMediator.Setup(m => m.Send(It.IsAny<GetOAuthUrlQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await controller.GetOAuthUrl(queryParams, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult.Value, Is.EqualTo(response));
        }

        [Test]
        public async Task LoginOAuth_ValidRequest_ReturnsOkWithResponse()
        {
            // Arrange
            var request = new UserOAuthenticationRequest();
            var response = new UserAuthenticationResponse { Email = "someemail" };

            mockMediator.Setup(m => m.Send(It.IsAny<LoginOAuthCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            // Act
            var result = await controller.LoginOAuth(request, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            Assert.That(okResult.Value, Is.EqualTo(response));
        }
    }
}
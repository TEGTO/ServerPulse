using Helper.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Authentication.OAuth.GitHub.Tests
{
    [TestFixture]
    internal class GitHubApiClientTests
    {
        private Mock<IHttpHelper> httpHelperMock;
        private Mock<IOptions<GitHubOAuthSettings>> optionsMock;
        private GitHubApiClient gitHubApiClient;
        private GitHubOAuthSettings gitHubOAuthSettings;

        [SetUp]
        public void SetUp()
        {
            httpHelperMock = new Mock<IHttpHelper>();
            optionsMock = new Mock<IOptions<GitHubOAuthSettings>>();
            gitHubOAuthSettings = new GitHubOAuthSettings
            {
                AppName = "TestApp"
            };
            optionsMock.Setup(o => o.Value).Returns(gitHubOAuthSettings);

            gitHubApiClient = new GitHubApiClient(optionsMock.Object, httpHelperMock.Object);
        }

        [Test]
        public async Task GetUserInfoAsync_ValidAccessToken_ReturnsGitHubUserResult()
        {
            // Arrange
            var accessToken = "valid_token";
            var cancellationToken = CancellationToken.None;
            var expectedEndpoint = "https://api.github.com/user";

            var expectedResponse = new GitHubUserResult
            {
                Id = 123,
                Login = "testuser",
                Email = "testuser@example.com"
            };

            httpHelperMock
                .Setup(h => h.SendGetRequestAsync<GitHubUserResult>(
                    expectedEndpoint,
                    It.IsAny<Dictionary<string, string>>(),
                    It.IsAny<Dictionary<string, string>>(),
                    accessToken,
                    cancellationToken))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await gitHubApiClient.GetUserInfoAsync(accessToken, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(expectedResponse.Id));
            Assert.That(result.Login, Is.EqualTo(expectedResponse.Login));
            Assert.That(result.Email, Is.EqualTo(expectedResponse.Email));

            httpHelperMock.Verify(h => h.SendGetRequestAsync<GitHubUserResult>(
                expectedEndpoint,
                It.IsAny<Dictionary<string, string>>(),
                It.Is<Dictionary<string, string>>(headers => headers.ContainsKey("User-Agent") && headers["User-Agent"] == "TestApp"),
                accessToken,
                cancellationToken), Times.Once);
        }
    }
}
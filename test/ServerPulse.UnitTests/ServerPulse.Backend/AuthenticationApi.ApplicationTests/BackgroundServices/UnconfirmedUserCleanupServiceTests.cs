using AuthenticationApi.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using MockQueryable.Moq;
using Moq;

namespace AuthenticationApi.Application.BackgroundServices.Tests
{
    [TestFixture]
    internal class UnconfirmedUserCleanupServiceTests
    {
        private Mock<UserManager<User>> userManagerMock;
        private Mock<IFeatureManager> featureManagerMock;
        private Mock<ILogger<UnconfirmedUserCleanupService>> loggerMock;
        private UnconfirmedUserCleanupService cleanupService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            featureManagerMock = new Mock<IFeatureManager>();
            loggerMock = new Mock<ILogger<UnconfirmedUserCleanupService>>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { ConfigurationKeys.UNCONRFIRMED_USERS_CLEANUP_IN_MINUTES, "60" }
                })
                .Build();

            cleanupService = new UnconfirmedUserCleanupService(userManagerMock.Object, featureManagerMock.Object, loggerMock.Object, configuration);
        }

        private static IEnumerable<TestCaseData> UnconfirmedUserTestCases()
        {
            var now = DateTime.UtcNow;

            yield return new TestCaseData(
                true,
                new[]
                {
                    new User { Email = "user1@example.com", EmailConfirmed = false, RegisteredAtUtc = now.AddMinutes(-61) },
                    new User { Email = "user2@example.com", EmailConfirmed = false, RegisteredAtUtc = now.AddMinutes(-30) }
                },
                new[] { "user1@example.com" },
                false
            ).SetDescription("Deletes expired unconfirmed users when feature enabled");

            yield return new TestCaseData(
               true,
               new[]
               {
                    new User { Email = "user1@example.com", EmailConfirmed = false, RegisteredAtUtc = now.AddMinutes(-61) },
                    new User { Email = "user2@example.com", EmailConfirmed = false, RegisteredAtUtc = now.AddMinutes(-30) }
               },
               new[] { "user1@example.com" },
               true
           ).SetDescription("Unconfirmed user deletion is failure");

            yield return new TestCaseData(
                false,
                new[]
                {
                    new User { Email = "user3@example.com", EmailConfirmed = false, RegisteredAtUtc = now.AddMinutes(-61) }
                },
                Array.Empty<string>(),
                false
            ).SetDescription(" Skips cleanup when feature disabled");
        }

        [Test]
        [TestCaseSource(nameof(UnconfirmedUserTestCases))]
        public async Task CleanupUnconfirmedUsersAsync_TestCases(
            bool isFeatureEnabled,
            User[] users,
            string[] expectedDeletions,
            bool isDeletionFailed)
        {
            // Arrange
            featureManagerMock.Setup(x => x.IsEnabledAsync(Features.EMAIL_CONFIRMATION))
                .ReturnsAsync(isFeatureEnabled);

            userManagerMock.Setup(x => x.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);

            if (isDeletionFailed)
            {
                userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Failed());
            }
            else
            {
                userManagerMock.Setup(x => x.DeleteAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);
            }

            // Act
            await cleanupService.CleanupUnconfirmedUsersAsync();

            // Assert
            foreach (var email in expectedDeletions)
            {
                userManagerMock.Verify(x => x.DeleteAsync(It.Is<User>(u => u.Email == email)), Times.Once);
            }

            if (isDeletionFailed)
            {
                loggerMock.Verify(x =>
                    x.Log
                    (
                         LogLevel.Warning,
                         It.IsAny<EventId>(),
                         It.IsAny<It.IsAnyType>(),
                         It.IsAny<Exception>(),
                         It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                    Times.Exactly(expectedDeletions.Length)
                );
            }
        }
    }
}
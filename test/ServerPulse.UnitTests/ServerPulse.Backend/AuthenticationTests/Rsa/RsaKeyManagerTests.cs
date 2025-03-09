using Authentication.Rsa;
using Authentication.Token;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace AuthenticationTests.Rsa.Tests
{
    [TestFixture]
    internal class RsaKeyManagerTests
    {
        private Mock<IOptions<JwtSettings>> mockOptions;
        private IFixture fixture;
        private JwtSettings jwtSettings;

        [SetUp]
        public void Setup()
        {
            jwtSettings = new JwtSettings
            {
                PrivateKey = TestRsaKeys.PRIVATE_KEY,
                PublicKey = TestRsaKeys.PUBLIC_KEY
            };

            mockOptions = new Mock<IOptions<JwtSettings>>();
            mockOptions.Setup(o => o.Value).Returns(jwtSettings);

            fixture = new Fixture().Customize(new AutoMoqCustomization());
            fixture.Inject(mockOptions.Object);
        }

        [Test]
        public void Constructor_Should_LoadKeys()
        {
            // Act
            var manager = fixture.Create<RsaKeyManager>();

            // Assert
            Assert.NotNull(manager.PublicKey);
            Assert.NotNull(manager.PrivateKey);
        }

        [Test]
        public void ReloadKeys_Should_Dispose_And_Reload()
        {
            // Arrange
            var manager = fixture.Create<RsaKeyManager>();
            var initialPublicKey = manager.PublicKey;
            var initialPrivateKey = manager.PrivateKey;

            // Act
            manager.ReloadKeys(jwtSettings);

            // Assert
            Assert.That(manager.PublicKey, Is.Not.SameAs(initialPublicKey));
            Assert.That(manager.PrivateKey, Is.Not.SameAs(initialPrivateKey));
        }

        [Test]
        public void LoadKeys_NoKeysInConfiguration_ShouldNotLoadKeys_And_LogInfo()
        {
            // Arrange
            jwtSettings = new JwtSettings
            {
                PrivateKey = "",
                PublicKey = "",
            };
            mockOptions.Setup(o => o.Value).Returns(jwtSettings);
            fixture.Inject(mockOptions.Object);

            var loggerMock = fixture.Freeze<Mock<ILogger<RsaKeyManager>>>();

            // Act
            var manager = fixture.Create<RsaKeyManager>();

            // Assert
            var publicRsa = manager.GetType().GetField("publicRsa", BindingFlags.NonPublic | BindingFlags.Instance);
            var privateRsa = manager.GetType().GetField("privateRsa", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNull(publicRsa!.GetValue(manager));
            Assert.IsNull(privateRsa!.GetValue(manager));

            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Exactly(2));
        }

        [Test]
        public void PublicKey_Should_Throw_When_Disposed()
        {
            // Arrange
            var manager = fixture.Create<RsaKeyManager>();

            // Act
            var prop = manager.GetType().GetField("publicRsa", BindingFlags.NonPublic | BindingFlags.Instance);
            prop!.SetValue(manager, null);

            // Assert
            Assert.Throws<ObjectDisposedException>(() => _ = manager.PublicKey);
        }

        [Test]
        public void PrivateKey_Should_Throw_When_Disposed()
        {
            // Arrange
            var manager = fixture.Create<RsaKeyManager>();

            // Act
            var prop = manager.GetType().GetField("privateRsa", BindingFlags.NonPublic | BindingFlags.Instance);
            prop!.SetValue(manager, null);

            // Assert
            Assert.Throws<ObjectDisposedException>(() => _ = manager.PrivateKey);
        }

        [Test]
        public void DisposeKeys_Should_Dispose_Keys()
        {
            // Arrange
            var manager = fixture.Create<RsaKeyManager>();

            // Act
            var method = manager.GetType().GetMethod("DisposeKeys", BindingFlags.NonPublic | BindingFlags.Instance);
            method!.Invoke(manager, []);

            // Assert
            Assert.Throws<ObjectDisposedException>(() => _ = manager.PublicKey);
            Assert.Throws<ObjectDisposedException>(() => _ = manager.PrivateKey);
        }
    }
}
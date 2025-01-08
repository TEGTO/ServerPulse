using Authentication.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationTests.Token
{
    [TestFixture]
    internal class JwtHandlerTests
    {
        private JwtHandler jwtHandler;

        [SetUp]
        public void Setup()
        {
            var jwtSettings = new JwtSettings
            {
                PrivateKey = TestRsaKeys.PRIVATE_KEY,
                PublicKey = TestRsaKeys.PUBLIC_KEY,
                Issuer = "test_issuer",
                Audience = "test_audience",
                ExpiryInMinutes = 30
            };

            var optionsMock = new Mock<IOptions<JwtSettings>>();
            optionsMock.Setup(x => x.Value).Returns(jwtSettings);

            jwtHandler = new JwtHandler(optionsMock.Object);
        }

        [Test]
        [TestCase("test@example.com", "testuser")]
        [TestCase("", "testuser")]
        [TestCase("", "")]
        [TestCase("user@example.com", "user123")]
        [TestCase("admin@example.com", "admin")]
        [TestCase("", "admin")]
        [TestCase("", "")]
        public void CreateToken_ValidData_ShouldReturnValidAccessToken(string email, string username)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, username),
            };

            // Act
            var accessTokenData = jwtHandler.CreateToken(claims);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(accessTokenData.AccessToken));
            Assert.IsFalse(string.IsNullOrEmpty(accessTokenData.RefreshToken));

            var tokenHandler = new JwtSecurityTokenHandler();
            Assert.IsTrue(tokenHandler.CanReadToken(accessTokenData.AccessToken));
        }

        [Test]
        [TestCase("test@example.com", "testuser")]
        [TestCase("", "testuser")]
        [TestCase("user@example.com", "user123")]
        [TestCase("admin@example.com", "admin")]
        [TestCase("", "admin")]
        [TestCase("", "")]
        public void GetPrincipalFromExpiredToken_ValidData_ValidPrincipal(string email, string username)
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, username),
            };

            var accessTokenData = jwtHandler.CreateToken(claims);

            // Act
            Assert.IsNotNull(accessTokenData.AccessToken);
            var principal = jwtHandler.GetPrincipalFromExpiredToken(accessTokenData.AccessToken);

            // Assert
            Assert.IsNotNull(principal);
            Assert.IsNotNull(principal.Identity);
            Assert.IsTrue(principal.Identity.IsAuthenticated);
        }

        [Test]
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("", typeof(ArgumentNullException))]
        [TestCase("invalid_jwt_token", typeof(SecurityTokenMalformedException))]
        public void GetPrincipalFromExpiredToken_InvalidData_ThrowsException(string? token, Type exceptionType)
        {
            // Act & Assert
            var exception = Assert.Throws(exceptionType, () => jwtHandler.GetPrincipalFromExpiredToken(token!));
            Assert.IsInstanceOf(exceptionType, exception);
        }
    }
}
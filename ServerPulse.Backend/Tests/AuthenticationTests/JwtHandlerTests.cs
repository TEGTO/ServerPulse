using Authentication.Configuration;
using Authentication.Models;
using Authentication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AuthenticationTests
{
    [TestFixture]
    internal class JwtHandlerTests
    {
        private Mock<JwtSettings> mockJwtSettings;

        [SetUp]
        public void Setup()
        {
            // Mock JwtSettings
            var jwtSettings = new JwtSettings
            {
                Key = "this is super secret key for authentication testing",
                Issuer = "test_issuer",
                Audience = "test_audience",
                ExpiryInMinutes = 30
            };

            mockJwtSettings = new Mock<JwtSettings>();
            mockJwtSettings.SetupGet(settings => settings.Key).Returns(jwtSettings.Key);
            mockJwtSettings.SetupGet(settings => settings.Issuer).Returns(jwtSettings.Issuer);
            mockJwtSettings.SetupGet(settings => settings.Audience).Returns(jwtSettings.Audience);
            mockJwtSettings.SetupGet(settings => settings.ExpiryInMinutes).Returns(jwtSettings.ExpiryInMinutes);
        }

        private JwtHandler CreateJwtHandler()
        {
            return new JwtHandler(
                this.mockJwtSettings.Object);
        }

        [Test]
        public void CreateToken_ValidData_ValidAccessToken()
        {
            // Arrange
            var user = new IdentityUser
            {
                Email = "test@example.com",
                UserName = "testuser"
            };
            var jwtHandler = this.CreateJwtHandler();
            // Act
            var accessTokenData = jwtHandler.CreateToken(user);
            // Assert
            Assert.IsNotNull(accessTokenData.AccessToken);
            Assert.IsNotNull(accessTokenData.RefreshToken);
            Assert.IsNotEmpty(accessTokenData.AccessToken);
            Assert.IsNotEmpty(accessTokenData.RefreshToken);
            mockJwtSettings.VerifyAll();
        }
        [Test]
        public void GetPrincipalFromExpiredToken_ValidData_ValidPrincipal()
        {
            // Arrange
            var user = new IdentityUser
            {
                Email = "test@example.com",
                UserName = "testuser"
            };
            var jwtHandler = this.CreateJwtHandler();
            var accessTokenData = jwtHandler.CreateToken(user);
            // Act
            var principal = jwtHandler.GetPrincipalFromExpiredToken(accessTokenData.AccessToken);
            // Assert
            Assert.IsNotNull(principal);
            Assert.IsTrue(principal.Identity.IsAuthenticated);
        }
        [Test]
        public void GetPrincipalFromExpiredToken_InvalidData_ThrowsException()
        {
            // Arrange
            var jwtHandler = this.CreateJwtHandler();
            var invalidToken = "invalid_jwt_token";
            // Act & Assert
            Assert.Throws<SecurityTokenMalformedException>(() => jwtHandler.GetPrincipalFromExpiredToken(invalidToken));
        }
        [Test]
        public void RefreshToken_ValidData_NewTokens()
        {
            // Arrange
            var user = new IdentityUser
            {
                Email = "test@example.com",
                UserName = "testuser"
            };
            var jwtHandler = this.CreateJwtHandler();
            var accessTokenData = jwtHandler.CreateToken(user);
            // Act
            var newAccessTokenData = jwtHandler.RefreshToken(user, new AccessTokenData { AccessToken = accessTokenData.AccessToken });
            // Assert
            Assert.IsNotNull(newAccessTokenData.AccessToken);
            Assert.IsNotNull(newAccessTokenData.RefreshToken);
            Assert.That(newAccessTokenData.AccessToken, Is.EqualTo(accessTokenData.AccessToken));
            //Will be equal, `cause current token is valid, so with the same data it will be the same
        }
        //[Test]
        //public void CreateToken_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();
        //    IdentityUser user = null;

        //    // Act
        //    var result = jwtHandler.CreateToken(
        //        user);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public void GetSigningCredentials_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();

        //    // Act
        //    var result = jwtHandler.GetSigningCredentials();

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public void GetClaims_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();
        //    IdentityUser user = null;

        //    // Act
        //    var result = jwtHandler.GetClaims(
        //        user);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public void GenerateTokenOptions_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();
        //    SigningCredentials signingCredentials = null;
        //    List claims = null;

        //    // Act
        //    var result = jwtHandler.GenerateTokenOptions(
        //        signingCredentials,
        //        claims);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public void GetPrincipalFromExpiredToken_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();
        //    string token = null;

        //    // Act
        //    var result = jwtHandler.GetPrincipalFromExpiredToken(
        //        token);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}

        //[Test]
        //public void RefreshToken_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var jwtHandler = this.CreateJwtHandler();
        //    IdentityUser user = null;
        //    AccessTokenData token = null;

        //    // Act
        //    var result = jwtHandler.RefreshToken(
        //        user,
        //        token);

        //    // Assert
        //    Assert.Fail();
        //    this.mockRepository.VerifyAll();
        //}
    }
}

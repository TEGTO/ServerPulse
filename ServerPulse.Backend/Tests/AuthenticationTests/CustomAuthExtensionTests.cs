using Authentication;
using Authentication.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text;

namespace AuthenticationTests
{
    [TestFixture]
    internal class CustomAuthExtensionTests
    {
        private IServiceCollection services;
        private Mock<JwtSettings> jwtSettingsMock;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            jwtSettingsMock = new Mock<JwtSettings>();
            jwtSettingsMock.Setup(s => s.Key).Returns("A very secret key");
            jwtSettingsMock.Setup(s => s.Issuer).Returns("TestIssuer");
            jwtSettingsMock.Setup(s => s.Audience).Returns("TestAudience");
        }

        [Test]
        public void AddCustomJwtAuthentication_ShouldConfigureAuthenticationSchemes()
        {
            //Arrange
            services.AddCustomJwtAuthentication(jwtSettingsMock.Object);
            //Act
            var serviceProvider = services.BuildServiceProvider();
            var authenticationOptions = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
            //Assert
            Assert.That(authenticationOptions.DefaultAuthenticateScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultChallengeScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
        }
        [Test]
        public void AddCustomJwtAuthentication_ShouldConfigureTokenValidationParameters()
        {
            //Arrange
            services.AddCustomJwtAuthentication(jwtSettingsMock.Object);
            //Act
            var serviceProvider = services.BuildServiceProvider();
            var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
            var tokenValidationParameters = jwtBearerOptions.TokenValidationParameters;
            //Assert
            Assert.That(tokenValidationParameters.ValidIssuer, Is.EqualTo(jwtSettingsMock.Object.Issuer));
            Assert.That(tokenValidationParameters.ValidAudience, Is.EqualTo(jwtSettingsMock.Object.Audience));
            Assert.That(tokenValidationParameters.IssuerSigningKey.KeyId, Is.EqualTo(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsMock.Object.Key)).KeyId));
            Assert.IsTrue(tokenValidationParameters.ValidateIssuer);
            Assert.IsTrue(tokenValidationParameters.ValidateAudience);
            Assert.IsTrue(tokenValidationParameters.ValidateLifetime);
            Assert.IsTrue(tokenValidationParameters.ValidateIssuerSigningKey);
        }
    }
}

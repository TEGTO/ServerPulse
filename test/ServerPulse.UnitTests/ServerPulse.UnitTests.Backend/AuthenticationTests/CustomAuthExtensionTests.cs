using Authentication;
using Authentication.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AuthenticationTests
{
    [TestFixture]
    internal class CustomAuthExtensionTests
    {
        private IServiceCollection services;
        private Mock<IConfiguration> configurationMock;
        private JwtSettings expectedJwtSettings;

        [SetUp]
        public void SetUp()
        {
            services = new ServiceCollection();
            configurationMock = new Mock<IConfiguration>();
            expectedJwtSettings = new JwtSettings
            {
                Key = "A very secret key",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryInMinutes = 60
            };

            configurationMock.Setup(c => c[JWTConfiguration.JWT_SETTINGS_KEY]).Returns(expectedJwtSettings.Key);
            configurationMock.Setup(c => c[JWTConfiguration.JWT_SETTINGS_AUDIENCE]).Returns(expectedJwtSettings.Audience);
            configurationMock.Setup(c => c[JWTConfiguration.JWT_SETTINGS_ISSUER]).Returns(expectedJwtSettings.Issuer);
            configurationMock.Setup(c => c[JWTConfiguration.JWT_SETTINGS_EXPIRY_IN_MINUTES]).Returns(expectedJwtSettings.ExpiryInMinutes.ToString());
        }

        [Test]
        public void ConfigureIdentityServices_ShouldAddJwtSettingsAsSingleton()
        {
            // Act
            services.ConfigureIdentityServices(configurationMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var jwtSettings = serviceProvider.GetRequiredService<JwtSettings>();
            Assert.That(jwtSettings.Key, Is.EqualTo(expectedJwtSettings.Key));
            Assert.That(jwtSettings.Issuer, Is.EqualTo(expectedJwtSettings.Issuer));
            Assert.That(jwtSettings.Audience, Is.EqualTo(expectedJwtSettings.Audience));
            Assert.That(jwtSettings.ExpiryInMinutes, Is.EqualTo(expectedJwtSettings.ExpiryInMinutes));
        }
        [Test]
        public void ConfigureIdentityServices_ShouldConfigureAuthorization()
        {
            // Act
            services.ConfigureIdentityServices(configurationMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var authorizationOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
            Assert.That(authorizationOptions, Is.Not.Null);
        }
        [Test]
        public void ConfigureIdentityServices_ShouldConfigureCustomJwtAuthentication()
        {
            // Act
            services.ConfigureIdentityServices(configurationMock.Object);
            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var authenticationOptions = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
            Assert.That(authenticationOptions.DefaultAuthenticateScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultChallengeScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
            var tokenValidationParameters = jwtBearerOptions.TokenValidationParameters;
            Assert.That(tokenValidationParameters.ValidIssuer, Is.EqualTo(expectedJwtSettings.Issuer));
            Assert.That(tokenValidationParameters.ValidAudience, Is.EqualTo(expectedJwtSettings.Audience));
            Assert.That(tokenValidationParameters.IssuerSigningKey, Is.TypeOf<SymmetricSecurityKey>());
            Assert.IsTrue(tokenValidationParameters.ValidateIssuer);
            Assert.IsTrue(tokenValidationParameters.ValidateAudience);
            Assert.IsTrue(tokenValidationParameters.ValidateLifetime);
            Assert.IsTrue(tokenValidationParameters.ValidateIssuerSigningKey);
        }
    }
}
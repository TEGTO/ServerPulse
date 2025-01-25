using Authentication.OAuth.GitHub;
using Authentication.OAuth.Google;
using Authentication.Rsa;
using Authentication.Token;
using AuthenticationTests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Authentication.Tests
{
    [TestFixture]
    internal class ExtensionTests
    {
        private IServiceCollection services;
        private IConfiguration configuration;
        private JwtSettings expectedJwtSettings;
        private GoogleOAuthSettings expectedGoogleOAuthSettings;

        [SetUp]
        public void SetUp()
        {
            var mockGitHubOAuthApi = new Mock<IGitHubOAuthApi>();
            var mockGoogleOAuthApi = new Mock<IGoogleOAuthApi>();

            services = new ServiceCollection();

            expectedJwtSettings = new JwtSettings
            {
                PrivateKey = TestRsaKeys.PRIVATE_KEY,
                PublicKey = TestRsaKeys.PUBLIC_KEY,
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryInMinutes = 60
            };

            expectedGoogleOAuthSettings = new GoogleOAuthSettings()
            {
                ClientId = "ClientId",
                ClientSecret = "Some google secret",
                Scope = "profile.com"
            };

            var inMemorySettings = new Dictionary<string, string>
            {
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PrivateKey)}", expectedJwtSettings.PrivateKey },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.PublicKey)}", expectedJwtSettings.PublicKey },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Audience)}", expectedJwtSettings.Audience },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.Issuer)}", expectedJwtSettings.Issuer },
                { $"{JwtSettings.SETTINGS_SECTION}:{nameof(JwtSettings.ExpiryInMinutes)}", expectedJwtSettings.ExpiryInMinutes.ToString() },

                { $"{GoogleOAuthSettings.SETTINGS_SECTION}:{nameof(GoogleOAuthSettings.ClientId)}", expectedGoogleOAuthSettings.ClientId },
                { $"{GoogleOAuthSettings.SETTINGS_SECTION}:{nameof(GoogleOAuthSettings.ClientSecret)}", expectedGoogleOAuthSettings.ClientSecret },
                { $"{GoogleOAuthSettings.SETTINGS_SECTION}:{nameof(GoogleOAuthSettings.Scope)}", expectedGoogleOAuthSettings.Scope },
            };

            configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();

            services.AddSingleton(configuration);
            services.AddSingleton(mockGitHubOAuthApi.Object);
            services.AddSingleton(mockGoogleOAuthApi.Object);

            services.AddAuthentication();
            services.AddAuthorization();
        }

        [Test]
        public void AddOAuthServices_ShouldAddOAuthSettingsAndServicesAsSingletons()
        {
            // Act
            services.AddOAuthServices(configuration);

            //Act
            var serviceProvider = services.BuildServiceProvider();
            var googleOAuthSettings = serviceProvider.GetRequiredService<IOptions<GoogleOAuthSettings>>().Value;
            var googleOAuthHttpClient = serviceProvider.GetRequiredService<IGoogleOAuthClient>();
            var googleTokenValidator = serviceProvider.GetRequiredService<IGoogleTokenValidator>();

            // Assert
            Assert.That(googleOAuthHttpClient, Is.Not.Null);

            Assert.That(googleTokenValidator, Is.Not.Null);

            Assert.That(googleOAuthSettings.ClientId, Is.EqualTo(expectedGoogleOAuthSettings.ClientId));
            Assert.That(googleOAuthSettings.ClientSecret, Is.EqualTo(expectedGoogleOAuthSettings.ClientSecret));
            Assert.That(googleOAuthSettings.Scope, Is.EqualTo(expectedGoogleOAuthSettings.Scope));
        }

        [Test]
        public void AddIdentity_ShouldAuthSettingsAsSingletons()
        {
            // Act
            services.AddIdentity(configuration);

            //Act
            var serviceProvider = services.BuildServiceProvider();
            var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;

            // Assert
            Assert.That(jwtSettings.PrivateKey, Is.EqualTo(expectedJwtSettings.PrivateKey));
            Assert.That(jwtSettings.PublicKey, Is.EqualTo(expectedJwtSettings.PublicKey));
            Assert.That(jwtSettings.Issuer, Is.EqualTo(expectedJwtSettings.Issuer));
            Assert.That(jwtSettings.Audience, Is.EqualTo(expectedJwtSettings.Audience));
            Assert.That(jwtSettings.ExpiryInMinutes, Is.EqualTo(expectedJwtSettings.ExpiryInMinutes));
        }

        [Test]
        public void AddIdentity_ShouldConfigureAuthorization()
        {
            // Act
            services.AddIdentity(configuration);

            //Act
            var serviceProvider = services.BuildServiceProvider();
            var authorizationOptions = serviceProvider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

            // Assert
            Assert.That(authorizationOptions, Is.Not.Null);
        }

        [Test]
        public void AddIdentity_ShouldConfigureJwtAuthentication()
        {
            // Act
            services.AddIdentity(configuration);

            //Act
            var serviceProvider = services.BuildServiceProvider();
            var authenticationOptions = serviceProvider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
            var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsSnapshot<JwtBearerOptions>>()
                .Get(JwtBearerDefaults.AuthenticationScheme);
            var tokenValidationParameters = jwtBearerOptions.TokenValidationParameters;

            // Assert
            Assert.That(authenticationOptions.DefaultAuthenticateScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultChallengeScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));
            Assert.That(authenticationOptions.DefaultScheme, Is.EqualTo(JwtBearerDefaults.AuthenticationScheme));

            Assert.That(tokenValidationParameters.ValidIssuer, Is.EqualTo(expectedJwtSettings.Issuer));
            Assert.That(tokenValidationParameters.ValidAudience, Is.EqualTo(expectedJwtSettings.Audience));
            Assert.That(tokenValidationParameters.IssuerSigningKey, Is.TypeOf<RsaSecurityKey>());

            Assert.IsTrue(tokenValidationParameters.ValidateIssuer);
            Assert.IsTrue(tokenValidationParameters.ValidateAudience);
            Assert.IsTrue(tokenValidationParameters.ValidateLifetime);
            Assert.IsTrue(tokenValidationParameters.ValidateIssuerSigningKey);
        }

        [Test]
        public void AddIdentity_ShouldAddServices()
        {
            // Act
            services.AddIdentity(configuration);

            //Act
            var serviceProvider = services.BuildServiceProvider();
            var rsaKeyManager = serviceProvider.GetRequiredService<IRsaKeyManager>();
            var tokenHandler = serviceProvider.GetRequiredService<ITokenHandler>();

            // Assert
            Assert.That(rsaKeyManager, Is.Not.Null);
            Assert.That(tokenHandler, Is.Not.Null);
        }
    }
}
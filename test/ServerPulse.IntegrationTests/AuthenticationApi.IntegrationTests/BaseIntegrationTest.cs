using Authentication.OAuth.Google;
using Authentication.Token;
using AuthenticationApi.Application;
using AuthenticationApi.Core.Entities;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using IEmailSender = EmailControl.IEmailSender;

namespace AuthenticationApi.IntegrationTests
{
    internal abstract class BaseIntegrationTest
    {
        protected HttpClient client;
        protected JwtSettings settings;
        protected UserManager<User> userManager;
        protected bool isConfirmEmailEnabled;
        protected bool isOAuthEnabled;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        private IServiceScope scope;
        protected Mock<IGoogleOAuthClient>? mockGoogleOAuthHttpClient;
        protected Mock<IGoogleTokenValidator>? mockGoogleTokenValidator;
        protected Mock<IBackgroundJobClient>? mockBackgroundJobClient;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();
            factory = (await wrapper.GetFactoryAsync()).WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IGoogleOAuthClient));
                    services.RemoveAll(typeof(IGoogleTokenValidator));
                    services.RemoveAll(typeof(IEmailSender));
                    services.RemoveAll(typeof(IBackgroundJobClient));

                    mockGoogleOAuthHttpClient = new Mock<IGoogleOAuthClient>();
                    mockGoogleOAuthHttpClient.Setup(x => x.ExchangeAuthorizationCodeAsync(
                        "somecode",
                        It.IsAny<string>(),
                        "someurl",
                        It.IsAny<CancellationToken>()
                    ))
                    .ReturnsAsync(new GoogleOAuthTokenResult());

                    mockGoogleOAuthHttpClient.Setup(x => x.ExchangeAuthorizationCodeAsync(
                        It.Is<string>(x => x != "somecode"),
                        It.IsAny<string>(),
                        It.Is<string>(x => x != "someurl"),
                        It.IsAny<CancellationToken>()
                    ))
                    .ThrowsAsync(new InvalidDataException());

                    var expectedUrl = "https://oauth.example.com/auth?client_id=someClientId&redirect_uri=someurl&response_type=code&scope=email&code_challenge=hashedVerifier&code_challenge_method=S256&access_type=offline";

                    mockGoogleOAuthHttpClient.Setup(x => x.GenerateOAuthRequestUrl(
                        "someurl",
                        It.IsAny<string>(),
                        It.IsAny<string>()
                    )).Returns(expectedUrl);

                    mockGoogleTokenValidator = new Mock<IGoogleTokenValidator>();
                    mockGoogleTokenValidator.Setup(x => x.ValidateAsync(
                        It.IsAny<string>()
                    ))
                    .ReturnsAsync(new Payload
                    {
                        Email = "someemail@gmail.com",
                        Subject = "someloginprovidersubject"
                    });

                    var mockEmailSender = new Mock<IEmailSender>();

                    mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

                    services.AddScoped(_ => mockGoogleOAuthHttpClient.Object);
                    services.AddScoped(_ => mockGoogleTokenValidator.Object);
                    services.AddScoped(_ => mockEmailSender.Object);
                    services.AddScoped(_ => mockBackgroundJobClient.Object);
                });
            });

            InitializeServices();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            scope.Dispose();
            client.Dispose();
            userManager.Dispose();
            await factory.DisposeAsync();
            await wrapper.DisposeAsync();
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            settings = factory.Services.GetRequiredService<IOptions<JwtSettings>>().Value;

            var scopedServices = scope.ServiceProvider;
            userManager = scopedServices.GetRequiredService<UserManager<User>>();

            var configuration = factory.Services.GetService<IConfiguration>();
            if (configuration != null)
            {
                isConfirmEmailEnabled = bool.Parse(configuration[$"FeatureManagement:{Features.EMAIL_CONFIRMATION}"]!);
                isOAuthEnabled = bool.Parse(configuration[$"FeatureManagement:{Features.OAUTH}"]!);
            }
        }
    }
}
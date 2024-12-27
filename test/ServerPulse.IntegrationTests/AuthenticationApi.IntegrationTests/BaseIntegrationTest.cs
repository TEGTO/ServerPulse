using Authentication.OAuth.Google;
using Authentication.Token;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.IntegrationTests
{
    [TestFixture]
    public abstract class BaseIntegrationTest
    {
        protected HttpClient client;
        protected JwtSettings settings;
        private WebAppFactoryWrapper wrapper;
        private WebApplicationFactory<Program> factory;
        protected Mock<IGoogleOAuthHttpClient>? mockGoogleOAuthHttpClient;
        protected Mock<IGoogleTokenValidator>? mockGoogleTokenValidator;
        private IServiceScope scope;

        [OneTimeSetUp]
        public async Task GlobalSetup()
        {
            wrapper = new WebAppFactoryWrapper();
            factory = (await wrapper.GetFactoryAsync()).WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IGoogleOAuthHttpClient));
                    services.RemoveAll(typeof(IGoogleTokenValidator));

                    mockGoogleOAuthHttpClient = new Mock<IGoogleOAuthHttpClient>();
                    mockGoogleOAuthHttpClient.Setup(x => x.ExchangeAuthorizationCodeAsync(
                        "somecode",
                        "someverifier",
                        "someurl",
                        It.IsAny<CancellationToken>()
                    ))
                    .ReturnsAsync(new GoogleOAuthTokenResult());

                    mockGoogleOAuthHttpClient.Setup(x => x.ExchangeAuthorizationCodeAsync(
                        It.Is<string>(x => x != "somecode"),
                        It.Is<string>(x => x != "someverifier"),
                        It.Is<string>(x => x != "someurl"),
                        It.IsAny<CancellationToken>()
                    ))
                    .ThrowsAsync(new InvalidDataException());

                    var expectedUrl = "https://oauth.example.com/auth?client_id=someClientId&redirect_uri=someurl&response_type=code&scope=email&code_challenge=hashedVerifier&code_challenge_method=S256&access_type=offline";

                    mockGoogleOAuthHttpClient.Setup(x => x.GenerateOAuthRequestUrl(
                        It.IsAny<string>(),
                        "someurl",
                        "someverifier"
                    )).Returns(expectedUrl);

                    mockGoogleTokenValidator = new Mock<IGoogleTokenValidator>();
                    mockGoogleTokenValidator.Setup(x => x.ValidateAsync(
                        It.IsAny<string>(),
                        It.IsAny<ValidationSettings>()
                    ))
                    .ReturnsAsync(new Payload
                    {
                        Email = "someemail@gmail.com",
                        Subject = "someloginprovidersubject"
                    });

                    services.AddScoped(_ => mockGoogleOAuthHttpClient.Object);
                    services.AddScoped(_ => mockGoogleTokenValidator.Object);
                });
            });

            InitializeServices();
        }

        [OneTimeTearDown]
        public async Task GlobalTearDown()
        {
            scope.Dispose();
            client.Dispose();
            await factory.DisposeAsync();
            await wrapper.DisposeAsync();
        }

        private void InitializeServices()
        {
            scope = factory.Services.CreateScope();
            client = factory.CreateClient();
            settings = factory.Services.GetRequiredService<JwtSettings>();
        }
    }
}
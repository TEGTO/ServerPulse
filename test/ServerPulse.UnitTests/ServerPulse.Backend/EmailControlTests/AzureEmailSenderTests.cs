using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using Moq;

namespace EmailControl.Tests
{
    [TestFixture]
    internal class AzureEmailSenderTests
    {
        private Mock<IAzureEmailClientFacade> mockAzureEmailClient;
        private IOptions<AzureEmailSettings> mockOptions;
        private AzureEmailSender emailSender;

        [SetUp]
        public void SetUp()
        {
            var emailSettings = new AzureEmailSettings
            {
                ConnectionString = "fake-connection-string",
                SenderAddress = "sender@example.com"
            };

            mockOptions = Options.Create(emailSettings);
            mockAzureEmailClient = new Mock<IAzureEmailClientFacade>();

            emailSender = new AzureEmailSender(mockOptions, mockAzureEmailClient.Object);
        }

        [TestCase("recipient@example.com", "Test Subject", "<p>Test Body</p>", "Test Body")]
        [TestCase("user@domain.com", "Welcome!", "Hello <b>user</b>!", "Hello user!")]
        [TestCase("example@company.com", "Notification", "<div>Update received</div>", "Update received")]
        public async Task SendEmailAsync_ShouldCallEmailClient_WithCorrectParameters(string toEmail, string subject, string body, string expectedPlainText)
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            // Act
            await emailSender.SendEmailAsync(toEmail, subject, body, cancellationToken);

            // Assert
            mockAzureEmailClient.Verify(client => client.SendAsync(
                WaitUntil.Completed,
                It.Is<EmailMessage>(x => x.Content.PlainText == expectedPlainText && x.Content.Html == body),
                cancellationToken), Times.Once);
        }
    }
}
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;
using Moq;

namespace EmailControl.Tests
{
    [TestFixture]
    internal class EmailSenderTests
    {
        private Mock<IEmailClientWrapper> mockEmailClient;
        private IOptions<EmailSettings> mockOptions;
        private EmailSender emailSender;

        private EmailSettings emailSettings;

        [SetUp]
        public void SetUp()
        {
            emailSettings = new EmailSettings
            {
                ConnectionString = "fake-connection-string",
                SenderAddress = "sender@example.com"
            };

            mockOptions = Options.Create(emailSettings);
            mockEmailClient = new Mock<IEmailClientWrapper>(MockBehavior.Strict);

            emailSender = new EmailSender(mockOptions, mockEmailClient.Object);
        }

        [TestCase("recipient@example.com", "Test Subject", "<p>Test Body</p>", "Test Body")]
        [TestCase("user@domain.com", "Welcome!", "Hello <b>user</b>!", "Hello user!")]
        [TestCase("example@company.com", "Notification", "<div>Update received</div>", "Update received")]
        public async Task SendEmailAsync_ShouldCallEmailClient_WithCorrectParameters(string toEmail, string subject, string body, string expectedPlainText)
        {
            // Arrange
            var cancellationToken = CancellationToken.None;

            mockEmailClient
                .Setup(client => client.SendAsync(
                    WaitUntil.Completed,
                    It.IsAny<EmailMessage>(),
                    cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await emailSender.SendEmailAsync(toEmail, subject, body, cancellationToken);

            // Assert
            mockEmailClient.Verify(client => client.SendAsync(
                WaitUntil.Completed,
                It.IsAny<EmailMessage>(),
                cancellationToken), Times.Once);
        }
    }
}
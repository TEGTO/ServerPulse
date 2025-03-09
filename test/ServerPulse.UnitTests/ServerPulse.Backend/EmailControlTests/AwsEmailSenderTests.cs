using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Moq;

namespace EmailControl.Tests
{
    [TestFixture]
    internal class AwsEmailSenderTests
    {
        private Mock<IAwsEmailClientFacade> mockAwsEmailClient;
        private IOptions<AwsEmailSettings> mockOptions;
        private AwsEmailSender emailSender;

        [SetUp]
        public void SetUp()
        {
            var emailSettings = new AwsEmailSettings
            {
                SenderAddress = "sender@example.com"
            };

            mockOptions = Options.Create(emailSettings);
            mockAwsEmailClient = new Mock<IAwsEmailClientFacade>();

            emailSender = new AwsEmailSender(mockOptions, mockAwsEmailClient.Object);
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
            mockAwsEmailClient.Verify(client => client.SendEmailAsync(
                It.Is<SendEmailRequest>(x => x.Message.Body.Text.Data == expectedPlainText && x.Message.Body.Html.Data == body),
                cancellationToken), Times.Once);
        }
    }
}
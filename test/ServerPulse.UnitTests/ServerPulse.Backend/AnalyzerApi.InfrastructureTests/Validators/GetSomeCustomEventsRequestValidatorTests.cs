using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetSomeCustomEvents;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class GetSomeCustomEventsRequestValidatorTests
    {
        private GetSomeCustomEventsRequestValidator validator;
        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[ConfigurationKeys.MAX_EVENT_AMOUNT_PER_REQUEST]).Returns("100");

            validator = new GetSomeCustomEventsRequestValidator(mockConfiguration.Object);
        }

        private static IEnumerable<TestCaseData> KeyValidationTestCases()
        {
            yield return new TestCaseData(null, false).SetDescription("Fails when Key is null.");
            yield return new TestCaseData("", false).SetDescription("Fails when Key is empty.");
            yield return new TestCaseData(new string('A', 257), false).SetDescription("Fails when Key exceeds maximum length.");
            yield return new TestCaseData("ValidKey", true).SetDescription("Passes when Key is valid.");
        }

        [Test]
        [TestCaseSource(nameof(KeyValidationTestCases))]
        public void Validate_KeyValidationCases(string key, bool isValid)
        {
            // Arrange
            var request = new GetSomeCustomEventsRequest { Key = key, NumberOfMessages = 50, StartDate = DateTime.UtcNow };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.Key);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.Key);
            }
        }

        private static IEnumerable<TestCaseData> NumberOfMessagesValidationTestCases()
        {
            yield return new TestCaseData(-1, false).SetDescription("Fails when NumberOfMessages is less than 0.");
            yield return new TestCaseData(0, false).SetDescription("Fails when NumberOfMessages is 0.");
            yield return new TestCaseData(101, false).SetDescription("Fails when NumberOfMessages exceeds the max allowed value.");
            yield return new TestCaseData(50, true).SetDescription("Passes when NumberOfMessages is within range.");
        }

        [Test]
        [TestCaseSource(nameof(NumberOfMessagesValidationTestCases))]
        public void Validate_NumberOfMessagesValidationCases(int numberOfMessages, bool isValid)
        {
            // Arrange
            var request = new GetSomeCustomEventsRequest { Key = "ValidKey", NumberOfMessages = numberOfMessages, StartDate = DateTime.UtcNow };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.NumberOfMessages);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.NumberOfMessages);
            }
        }

        [Test]
        public void Validate_ValidRequest_PassesValidation()
        {
            // Arrange
            var request = new GetSomeCustomEventsRequest
            {
                Key = "ValidKey",
                NumberOfMessages = 50,
                StartDate = DateTime.UtcNow,
                ReadNew = true
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
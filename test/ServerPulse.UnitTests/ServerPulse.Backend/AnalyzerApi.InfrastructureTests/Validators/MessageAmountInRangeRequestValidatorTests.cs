using AnalyzerApi.Infrastructure.Requests;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class MessageAmountInRangeRequestValidatorTests
    {
        private MessageAmountInRangeRequestValidator validator;
        private Mock<IConfiguration> mockConfiguration;

        [SetUp]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c[Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS]).Returns("60");

            validator = new MessageAmountInRangeRequestValidator(mockConfiguration.Object);
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
            var request = new MessageAmountInRangeRequest { Key = key, From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow, TimeSpan = TimeSpan.FromMinutes(10) };

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

        private static IEnumerable<TestCaseData> DateRangeValidationTestCases()
        {
            yield return new TestCaseData(0, -1, false).SetDescription("Fails when 'From' is after 'To'.");
            yield return new TestCaseData(-1, 0, true).SetDescription("Passes when 'From' is before 'To'.");
        }

        [Test]
        [TestCaseSource(nameof(DateRangeValidationTestCases))]
        public void Validate_DateRangeValidationCases(int addFromDays, int addToDays, bool isValid)
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "ValidKey",
                From = DateTime.UtcNow.AddDays(addFromDays),
                To = DateTime.UtcNow.AddDays(addToDays),
                TimeSpan = TimeSpan.FromMinutes(10)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.From);
                result.ShouldHaveValidationErrorFor(r => r.To);
            }
        }

        private static IEnumerable<TestCaseData> TimeSpanValidationTestCases()
        {
            yield return new TestCaseData(TimeSpan.FromSeconds(30), false).SetDescription("Fails when TimeSpan is less than the minimum.");
            yield return new TestCaseData(TimeSpan.FromSeconds(60), true).SetDescription("Passes when TimeSpan is equal to the minimum.");
            yield return new TestCaseData(TimeSpan.FromSeconds(120), true).SetDescription("Passes when TimeSpan is greater than the minimum.");
        }

        [Test]
        [TestCaseSource(nameof(TimeSpanValidationTestCases))]
        public void Validate_TimeSpanValidationCases(TimeSpan timeSpan, bool isValid)
        {
            // Arrange
            var request = new MessageAmountInRangeRequest { Key = "ValidKey", From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow, TimeSpan = timeSpan };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.TimeSpan);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.TimeSpan);
            }
        }

        [Test]
        public void Validate_ValidRequest_PassesValidation()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "ValidKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromMinutes(10)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
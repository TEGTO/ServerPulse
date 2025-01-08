using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using FluentValidation.TestHelper;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class GetLoadEventsInDataRangeRequestValidatorTests
    {
        private GetLoadEventsInDataRangeRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new GetLoadEventsInDataRangeRequestValidator();
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
            var request = new GetLoadEventsInDataRangeRequest { Key = key, From = DateTime.UtcNow.AddDays(-1), To = DateTime.UtcNow };

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
        public void Validate_DateRangeValidationCases(int addDaysFrom, int addDaysTo, bool isValid)
        {
            // Arrange
            var request = new GetLoadEventsInDataRangeRequest
            {
                Key = "ValidKey",
                From = DateTime.UtcNow.AddDays(addDaysFrom),
                To = DateTime.UtcNow.AddDays(addDaysTo)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(r => r.From);
                result.ShouldNotHaveValidationErrorFor(r => r.To);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(r => r.From);
                result.ShouldHaveValidationErrorFor(r => r.To);
            }
        }

        [Test]
        public void Validate_ValidRequest_PassesValidation()
        {
            // Arrange
            var request = new GetLoadEventsInDataRangeRequest
            {
                Key = "ValidKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
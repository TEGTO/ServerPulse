using FluentValidation.TestHelper;

namespace EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class LoadEventValidatorTests
    {
        private LoadEventValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new LoadEventValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // Test cases for Endpoint
            yield return new TestCaseData("ValidKey", null, "GET", 200, true, TimeSpan.Zero, false, "Endpoint")
                .SetDescription("Fails validation when Endpoint is null.");

            yield return new TestCaseData("ValidKey", "", "GET", 200, true, TimeSpan.Zero, false, "Endpoint")
                .SetDescription("Fails validation when Endpoint is empty.");

            yield return new TestCaseData("ValidKey", new string('A', 257), "GET", 200, true, TimeSpan.Zero, false, "Endpoint")
                .SetDescription("Fails validation when Endpoint exceeds maximum length.");

            // Test cases for Method
            yield return new TestCaseData("ValidKey", "https://api.example.com", null, 200, true, TimeSpan.Zero, false, "Method")
                .SetDescription("Fails validation when Method is null.");

            yield return new TestCaseData("ValidKey", "https://api.example.com", "", 200, true, TimeSpan.Zero, false, "Method")
                .SetDescription("Fails validation when Method is empty.");

            yield return new TestCaseData("ValidKey", "https://api.example.com", new string('B', 257), 200, true, TimeSpan.Zero, false, "Method")
                .SetDescription("Fails validation when Method exceeds maximum length.");

            // Test cases for StatusCode
            yield return new TestCaseData("ValidKey", "https://api.example.com", "GET", -1, true, TimeSpan.Zero, false, "StatusCode")
                .SetDescription("Fails validation when StatusCode is less than 0.");

            // Valid case
            yield return new TestCaseData("ValidKey", "https://api.example.com", "GET", 200, true, TimeSpan.Zero, true, null)
                .SetDescription("Passes validation when all properties are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(
            string key,
            string endpoint,
            string method,
            int statusCode,
            bool isValid,
            TimeSpan duration,
            bool shouldPass,
            string? errorProperty)
        {
            // Arrange
            var loadEvent = new LoadEvent(key, endpoint, method, statusCode, duration, DateTime.UtcNow);

            // Act
            var result = validator.TestValidate(loadEvent);

            // Assert
            if (shouldPass)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                Assert.IsNotNull(errorProperty);
                result.ShouldHaveValidationErrorFor(errorProperty!);
            }
        }
    }
}
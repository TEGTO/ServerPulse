using FluentValidation.TestHelper;

namespace EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class ConfigurationEventValidatorTests
    {
        private ConfigurationEventValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new ConfigurationEventValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(TimeSpan.FromSeconds(-1), false, "ServerKeepAliveInterval")
                .SetDescription("Fails validation when ServerKeepAliveInterval is negative.");

            yield return new TestCaseData(TimeSpan.Zero, true, null)
                .SetDescription("Passes validation when ServerKeepAliveInterval is zero.");

            yield return new TestCaseData(TimeSpan.FromSeconds(30), true, null)
                .SetDescription("Passes validation when ServerKeepAliveInterval is positive.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(TimeSpan serverKeepAliveInterval, bool isValid, string? errorProperty)
        {
            // Arrange
            var testEvent = new ConfigurationEvent("ValidKey", serverKeepAliveInterval);

            // Act
            var result = validator.TestValidate(testEvent);

            // Assert
            if (isValid)
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
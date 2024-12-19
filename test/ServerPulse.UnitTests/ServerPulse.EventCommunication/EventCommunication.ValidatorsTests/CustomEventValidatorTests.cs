using FluentValidation.TestHelper;

namespace EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class CustomEventValidatorTests
    {
        private CustomEventValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CustomEventValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            // Test cases for Name
            yield return new TestCaseData("ValidKey", null, "ValidDescription", false, "Name")
                .SetDescription("Fails validation when Name is null.");

            yield return new TestCaseData("ValidKey", "", "ValidDescription", false, "Name")
                .SetDescription("Fails validation when Name is empty.");

            yield return new TestCaseData("ValidKey", new string('A', 513), "ValidDescription", false, "Name")
                .SetDescription("Fails validation when Name exceeds maximum length.");

            // Test cases for Description
            yield return new TestCaseData("ValidKey", "ValidName", null, false, "Description")
                .SetDescription("Fails validation when Description is null.");

            yield return new TestCaseData("ValidKey", "ValidName", "", false, "Description")
                .SetDescription("Fails validation when Description is empty.");

            yield return new TestCaseData("ValidKey", "ValidName", new string('A', 1025), false, "Description")
                .SetDescription("Fails validation when Description exceeds maximum length.");

            // Valid case
            yield return new TestCaseData("ValidKey", "ValidName", "ValidDescription", true, null)
                .SetDescription("Passes validation when all properties are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(string key, string name, string description, bool isValid, string? errorProperty)
        {
            // Arrange
            var customEvent = new CustomEvent(key, name, description);

            // Act
            var result = validator.TestValidate(customEvent);

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
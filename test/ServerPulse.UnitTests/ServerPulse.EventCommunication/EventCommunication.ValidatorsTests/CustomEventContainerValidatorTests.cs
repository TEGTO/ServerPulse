using FluentValidation.TestHelper;

namespace EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class CustomEventContainerValidatorTests
    {
        private CustomEventContainerValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CustomEventContainerValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(false, "ValidSerializedEvent", false, "CustomEvent")
                .SetDescription("Fails validation when CustomEvent is null.");

            yield return new TestCaseData(true, null, false, "CustomEventSerialized")
                .SetDescription("Fails validation when CustomEventSerialized is null.");

            yield return new TestCaseData(true, "", false, "CustomEventSerialized")
                .SetDescription("Fails validation when CustomEventSerialized is empty.");

            yield return new TestCaseData(true, "ValidSerializedEvent", true, null)
                .SetDescription("Passes validation when both CustomEvent and CustomEventSerialized are valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(bool isCustomEventExists, string customEventSerialized, bool isValid, string? errorProperty)
        {
            // Arrange
            var container = new CustomEventContainer
            (
                isCustomEventExists ? new CustomEvent("ValidKey", "ValidName", "ValidDescription") : null!,
                customEventSerialized
            );

            // Act
            var result = validator.TestValidate(container);

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
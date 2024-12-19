using FluentValidation.TestHelper;

namespace EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class BaseEventValidatorTests
    {
        private BaseEventValidator<BaseEvent> validator;

        [SetUp]
        public void SetUp()
        {
            validator = new BaseEventValidator<BaseEvent>();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(null, false, "Key")
                .SetDescription("Fails validation when Key is null.");

            yield return new TestCaseData("", false, "Key")
                .SetDescription("Fails validation when Key is empty.");

            yield return new TestCaseData(new string('A', 257), false, "Key")
                .SetDescription("Fails validation when Key exceeds maximum length.");

            yield return new TestCaseData("ValidKey", true, null)
                .SetDescription("Passes validation when Key is valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationCases(string key, bool isValid, string? errorProperty)
        {
            // Arrange
            var testEvent = new TestEvent(key);

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

        private record TestEvent : BaseEvent
        {
            public TestEvent(string key) : base(key)
            {
            }
        }
    }
}
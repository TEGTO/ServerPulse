using FluentValidation.TestHelper;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class CheckSlotKeyRequestValidatorTests
    {
        private CheckSlotKeyRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CheckSlotKeyRequestValidator();
        }

        private static IEnumerable<TestCaseData> ValidationTestCases()
        {
            yield return new TestCaseData(null, false, "Fails when SlotKey is null.")
                .SetDescription("SlotKey cannot be null.");

            yield return new TestCaseData(string.Empty, false, "Fails when SlotKey is empty.")
                .SetDescription("SlotKey cannot be empty.");

            yield return new TestCaseData(new string('A', 257), false, "Fails when SlotKey exceeds maximum length.")
                .SetDescription("SlotKey cannot exceed 256 characters.");

            yield return new TestCaseData(new string('B', 256), true, "Passes when SlotKey is exactly at maximum length.")
                .SetDescription("SlotKey at maximum length is valid.");

            yield return new TestCaseData("ValidSlotKey", true, "Passes when SlotKey is valid.")
                .SetDescription("SlotKey with valid length and content is valid.");
        }

        [Test]
        [TestCaseSource(nameof(ValidationTestCases))]
        public void Validate_ValidationTestCases(string slotKey, bool isValid, string description)
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = slotKey };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            if (isValid)
            {
                result.ShouldNotHaveAnyValidationErrors();
            }
            else
            {
                result.ShouldHaveValidationErrorFor(x => x.SlotKey);
            }
        }
    }
}
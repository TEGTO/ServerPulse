using FluentValidation.TestHelper;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class CheckServerSlotRequestValidatorTests
    {
        private CheckServerSlotRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CheckServerSlotRequestValidator();
        }

        private static IEnumerable<TestCaseData> SlotKeyValidationTestCases()
        {
            yield return new TestCaseData(null, false, "SlotKey")
                .SetDescription("SlotKey is null; validation should fail.");
            yield return new TestCaseData("", false, "SlotKey")
                .SetDescription("SlotKey is empty; validation should fail.");
            yield return new TestCaseData(new string('A', 257), false, "SlotKey")
                .SetDescription("SlotKey exceeds maximum length of 256; validation should fail.");
            yield return new TestCaseData("ValidSlotKey123", true, null)
                .SetDescription("SlotKey is valid; validation should pass.");
            yield return new TestCaseData(new string('A', 256), true, null)
                .SetDescription("SlotKey is exactly 256 characters long; validation should pass.");
        }

        [Test]
        [TestCaseSource(nameof(SlotKeyValidationTestCases))]
        public void Validate_SlotKeyValidationCases(string slotKey, bool isValid, string? errorProperty)
        {
            // Arrange
            var request = new CheckSlotKeyRequest
            {
                SlotKey = slotKey
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
                Assert.IsNotNull(errorProperty);
                result.ShouldHaveValidationErrorFor(errorProperty!);
            }
        }
    }
}
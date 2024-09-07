using FluentValidation.TestHelper;
using ServerSlotApi.Validators;
using Shared.Dtos.ServerSlot;

namespace ServerSlotApiTests.Validators
{
    internal class CheckSlotKeyRequestValidatorTests
    {
        private CheckSlotKeyRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CheckSlotKeyRequestValidator();
        }

        [Test]
        public void Validate_SlotKeyIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void Validate_SlotKeyTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckSlotKeyRequest { SlotKey = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
    }
}

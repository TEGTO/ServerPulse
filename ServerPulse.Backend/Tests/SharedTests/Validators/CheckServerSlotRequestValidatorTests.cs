using FluentValidation.TestHelper;
using Shared.Dtos.ServerSlot;
using Shared.Validators;

namespace SharedTests.Validators
{
    [TestFixture]
    public class CheckServerSlotRequestValidatorTests
    {
        private CheckServerSlotRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new CheckServerSlotRequestValidator();
        }

        [Test]
        public void ValidateSlotKey_SlotKeyIsNull_ValidationError()
        {
            // Arrange
            var model = new CheckSlotKeyRequest { SlotKey = null };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new CheckSlotKeyRequest { SlotKey = string.Empty };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsTooLong_ValidationError()
        {
            // Arrange
            var longSlotKey = new string('x', 257);
            var model = new CheckSlotKeyRequest { SlotKey = longSlotKey };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new CheckSlotKeyRequest { SlotKey = "ValidSlotKey" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SlotKey);
        }
    }
}

using FluentValidation.TestHelper;
using Shared.Dtos.ServerEvent;
using Shared.Validators;

namespace SharedTests.Validators
{
    [TestFixture]
    public class SlotKeyDeletionEventValidatorTests
    {
        private SlotKeyDeletionEventValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new SlotKeyDeletionEventValidator();
        }

        [Test]
        public void ValidateSlotKey_SlotKeyIsNull_ValidationError()
        {
            // Arrange
            var model = new SlotKeyDeletionEvent(null);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new SlotKeyDeletionEvent(string.Empty);
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
            var model = new SlotKeyDeletionEvent(longSlotKey);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new SlotKeyDeletionEvent("ValidSlotKey");
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SlotKey);
        }
    }
}
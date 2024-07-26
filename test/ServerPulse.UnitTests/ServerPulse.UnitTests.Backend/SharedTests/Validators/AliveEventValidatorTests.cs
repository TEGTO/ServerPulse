using FluentValidation.TestHelper;
using Shared.Dtos.ServerEvent;
using Shared.Validators;

namespace SharedTests.Validators
{
    [TestFixture]
    public class AliveEventValidatorTests
    {
        private AliveEventValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new AliveEventValidator();
        }

        [Test]
        public void ValidateSlotKey_SlotKeyIsNull_ValidationError()
        {
            // Arrange
            var model = new AliveEvent(null, true);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new AliveEvent(string.Empty, true);
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
            var model = new AliveEvent(longSlotKey, true);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotKey);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new AliveEvent("ValidSlotKey", true);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SlotKey);
        }
    }
}

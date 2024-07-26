using EventCommunication.Validators;
using FluentValidation.TestHelper;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunicationTests.Validators
{
    [TestFixture]
    internal class AliveEventValidatorTests
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
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new AliveEvent(string.Empty, true);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
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
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateSlotKey_SlotKeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new AliveEvent("ValidSlotKey", true);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Key);
        }
    }
}
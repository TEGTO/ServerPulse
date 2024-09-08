using FluentValidation.TestHelper;
using ServerPulse.EventCommunication.Events;
using System.Text.Json;

namespace ServerPulse.EventCommunication.Validators.Tests
{
    [TestFixture]
    internal class CustomEventWrapperValidatorTests
    {
        private CustomEventWrapperValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new CustomEventWrapperValidator();
        }

        [Test]
        public void ValidateCustomEvent_CustomEventIsInvalid_ValidationError()
        {
            // Arrange
            var model = new CustomEventWrapper(null, "");
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomEvent);
        }
        [Test]
        public void ValidateCustomEvent_CustomEventSerializedIsInvalid_ValidationError()
        {
            // Arrange
            var model = new CustomEventWrapper(new CustomEvent("", "", ""), null);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomEventSerialized);
        }
        [Test]
        public void ValidateCustomEvent_CustomEventIsValid_NoValidationError()
        {
            // Arrange
            var ev = new CustomEvent("", "", "");
            var model = new CustomEventWrapper(ev, JsonSerializer.Serialize(ev));
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CustomEvent);
            result.ShouldNotHaveValidationErrorFor(x => x.CustomEventSerialized);
        }
    }
}
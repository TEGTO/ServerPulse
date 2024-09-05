using EventCommunication.Validators;
using FluentValidation.TestHelper;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunicationTests.Validators.Tests
{
    internal record class TestEvent(string Key) : BaseEvent(Key);

    [TestFixture]
    internal class BaseEventValidatorTests
    {
        private BaseEventValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new BaseEventValidator();
        }

        [Test]
        public void ValidateKey_KeyIsNull_ValidationError()
        {
            // Arrange
            var model = new TestEvent(null);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new TestEvent(string.Empty);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsTooLong_ValidationError()
        {
            // Arrange
            var longKey = new string('x', 257);
            var model = new TestEvent(longKey);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new TestEvent("ValidKey");
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Key);
        }
    }
}
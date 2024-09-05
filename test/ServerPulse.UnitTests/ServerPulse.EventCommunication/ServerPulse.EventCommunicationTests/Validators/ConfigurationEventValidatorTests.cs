using FluentValidation.TestHelper;
using ServerPulse.EventCommunication.Events;
using ServerPulse.EventCommunication.Validators;

namespace ServerPulse.EventCommunicationTests.Validators.Tests
{
    [TestFixture]
    internal class ConfigurationEventValidatorTests
    {
        private ConfigurationEventValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new ConfigurationEventValidator();
        }

        [Test]
        public void ValidateKey_KeyIsNull_ValidationError()
        {
            // Arrange
            var model = new ConfigurationEvent(null, TimeSpan.FromMinutes(5));
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new ConfigurationEvent(string.Empty, TimeSpan.FromMinutes(5));
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
            var model = new ConfigurationEvent(longKey, TimeSpan.FromMinutes(5));
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new ConfigurationEvent("ValidKey", TimeSpan.FromMinutes(5));
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Key);
        }
    }
}
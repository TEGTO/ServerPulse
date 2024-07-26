using FluentValidation.TestHelper;
using ServerPulse.EventCommunication.Events;
using ServerPulse.EventCommunication.Validators;

namespace ServerPulse.EventCommunicationTests.Validators
{
    [TestFixture]
    internal class LoadEventValidatorTests
    {
        private LoadEventValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new LoadEventValidator();
        }

        [Test]
        public void ValidateKey_KeyIsNull_ValidationError()
        {
            // Arrange
            var model = new LoadEvent(null, "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsEmpty_ValidationError()
        {
            // Arrange
            var model = new LoadEvent(string.Empty, "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
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
            var model = new LoadEvent(longKey, "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateKey_KeyIsValid_NoValidationError()
        {
            // Arrange
            var model = new LoadEvent("ValidKey", "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Key);
        }
        [Test]
        public void ValidateMethod_MethodIsNull_ValidationError()
        {
            // Arrange
            var model = new LoadEvent("key", "endpoint", null, 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Method);
        }
        [Test]
        public void ValidateMethod_MethodIsEmpty_ValidationError()
        {
            // Arrange
            var model = new LoadEvent("key", "endpoint", string.Empty, 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Method);
        }
        [Test]
        public void ValidateMethod_MethodIsTooLong_ValidationError()
        {
            // Arrange
            var longMethod = new string('x', 257);
            var model = new LoadEvent("key", "endpoint", longMethod, 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Method);
        }
        [Test]
        public void ValidateMethod_MethodIsValid_NoValidationError()
        {
            // Arrange
            var model = new LoadEvent("key", "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Method);
        }
        [Test]
        public void ValidateStatusCode_StatusCodeIsNegative_ValidationError()
        {
            // Arrange
            var model = new LoadEvent("key", "endpoint", "GET", -1, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StatusCode);
        }
        [Test]
        public void ValidateStatusCode_StatusCodeIsValid_NoValidationError()
        {
            // Arrange
            var model = new LoadEvent("key", "endpoint", "GET", 200, TimeSpan.FromSeconds(1), DateTime.Now);
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.StatusCode);
        }
    }
}
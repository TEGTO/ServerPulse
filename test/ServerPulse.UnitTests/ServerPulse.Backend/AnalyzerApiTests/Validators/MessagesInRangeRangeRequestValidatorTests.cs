using AnalyzerApi.Domain.Dtos.Requests;
using FluentValidation.TestHelper;

namespace AnalyzerApi.Validators.Tests
{
    [TestFixture]
    internal class MessagesInRangeRangeRequestValidatorTests
    {
        private MessagesInRangeRangeRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new MessagesInRangeRangeRequestValidator();
        }

        [Test]
        public void Validator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
        [Test]
        public void Validator_InvalidKey_Null_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = null,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidKey_Empty_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = string.Empty,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidKey_ExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = new string('a', 257), // Key exceeds 256 characters
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidFromAndToDate_FromIsAfterTo_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(-1)
            };

            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.From);
        }
        [Test]
        public void Validator_InvalidToDate_ToIsBeforeFrom_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow.AddDays(-2)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.To);
        }
    }
}
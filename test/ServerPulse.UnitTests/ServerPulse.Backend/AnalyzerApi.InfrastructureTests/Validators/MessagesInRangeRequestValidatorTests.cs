using AnalyzerApi.Infrastructure.Requests;
using FluentValidation.TestHelper;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class MessagesInRangeRequestValidatorTests
    {
        private MessagesInRangeRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new MessagesInRangeRequestValidator();
        }

        [Test]
        public void Validator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Validator_InvalidKey_Null_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = null!,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }

        [Test]
        public void Validator_InvalidKey_Empty_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = string.Empty,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }

        [Test]
        public void Validator_InvalidKey_ExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = new string('a', 257), // Key exceeds 256 characters
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }

        [Test]
        public void Validator_InvalidFromAndToDate_FromIsAfterTo_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.From);
        }

        [Test]
        public void Validator_InvalidToDate_ToIsBeforeFrom_FailsValidation()
        {
            // Arrange
            var request = new MessagesInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow.AddDays(-2)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.To);
        }
    }
}
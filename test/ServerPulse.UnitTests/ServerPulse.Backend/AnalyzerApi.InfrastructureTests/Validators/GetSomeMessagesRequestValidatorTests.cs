using AnalyzerApi.Infrastructure.Requests;
using FluentValidation.TestHelper;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class GetSomeMessagesRequestValidatorTests
    {
        private GetSomeMessagesRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new GetSomeMessagesRequestValidator();
        }

        [Test]
        public void Validator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "validKey",
                NumberOfMessages = 10
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
            var request = new GetSomeMessagesRequest
            {
                Key = null!,
                NumberOfMessages = 10
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
            var request = new GetSomeMessagesRequest
            {
                Key = string.Empty,
                NumberOfMessages = 10
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
            var request = new GetSomeMessagesRequest
            {
                Key = new string('a', 257), // Key exceeds 256 characters
                NumberOfMessages = 10
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }

        [Test]
        public void Validator_InvalidNumberOfMessages_LessThanOrEqualToZero_FailsValidation()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "validKey",
                NumberOfMessages = 0
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.NumberOfMessages);
        }

        [Test]
        public void Validator_InvalidNumberOfMessages_Negative_FailsValidation()
        {
            // Arrange
            var request = new GetSomeMessagesRequest
            {
                Key = "validKey",
                NumberOfMessages = -5
            };

            // Act 
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.NumberOfMessages);
        }
    }
}
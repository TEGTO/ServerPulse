using AnalyzerApi.Infrastructure.Requests;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApi.Infrastructure.Validators.Tests
{
    [TestFixture]
    internal class MessageAmountInRangeRequestValidatorTests
    {
        private MessageAmountInRangeRequestValidator validator;
        private Mock<IConfiguration> mockConfiguration;
        private const int MIN_STATISTICS_TIMESPAN_IN_SECONDS = 60;

        [SetUp]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS])
                .Returns(MIN_STATISTICS_TIMESPAN_IN_SECONDS.ToString());

            validator = new MessageAmountInRangeRequestValidator(mockConfiguration.Object);
        }

        [Test]
        public void Validator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
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
            var request = new MessageAmountInRangeRequest
            {
                Key = null!,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
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
            var request = new MessageAmountInRangeRequest
            {
                Key = string.Empty,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
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
            var request = new MessageAmountInRangeRequest
            {
                Key = new string('a', 257),
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
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
            var request = new MessageAmountInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(-1),
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
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
            var request = new MessageAmountInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow.AddDays(-2),
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.To);
        }

        [Test]
        public void Validator_InvalidTimeSpan_TooShort_FailsValidation()
        {
            // Arrange
            var request = new MessageAmountInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS - 1)
            };

            // Act
            var result = validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(r => r.TimeSpan);
        }
    }
}
using AnalyzerApi;
using AnalyzerApi.Domain.Dtos.Requests;
using AnalyzerApi.Validators;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using Moq;

namespace AnalyzerApiTests.Validators
{
    [TestFixture]
    internal class LoadAmountStatisticsInRangeRequestValidatorTests
    {
        private LoadAmountStatisticsInRangeRequestValidator validator;
        private Mock<IConfiguration> mockConfiguration;
        private const int MIN_STATISTICS_TIMESPAN_IN_SECONDS = 60;

        [SetUp]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(c => c[Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS])
                .Returns(MIN_STATISTICS_TIMESPAN_IN_SECONDS.ToString());

            validator = new LoadAmountStatisticsInRangeRequestValidator(mockConfiguration.Object);
        }

        [Test]
        public void Validator_ValidInput_PassesValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
        [Test]
        public void Validator_InvalidKey_Null_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = null,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidKey_Empty_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = string.Empty,
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidKey_ExceedsMaxLength_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = new string('a', 257),
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.Key);
        }
        [Test]
        public void Validator_InvalidFromAndToDate_FromIsAfterTo_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow,
                To = DateTime.UtcNow.AddDays(-1),
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.From);
        }
        [Test]
        public void Validator_InvalidToDate_ToIsBeforeFrom_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow.AddDays(-2),
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS + 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.To);
        }
        [Test]
        public void Validator_InvalidTimeSpan_TooShort_FailsValidation()
        {
            // Arrange
            var request = new LoadAmountStatisticsInRangeRequest
            {
                Key = "validKey",
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow,
                TimeSpan = TimeSpan.FromSeconds(MIN_STATISTICS_TIMESPAN_IN_SECONDS - 1)
            };
            // Act & Assert
            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(r => r.TimeSpan);
        }
    }
}

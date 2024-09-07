using FluentValidation.TestHelper;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Validators;

namespace ServerSlotApiTests.Validators
{
    [TestFixture]
    internal class UpdateServerSlotRequestValidatorTests
    {
        private UpdateServerSlotRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UpdateServerSlotRequestValidator();
        }

        [Test]
        public void Validate_NameIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UpdateServerSlotRequest { Name = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
        [Test]
        public void Validate_NameTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UpdateServerSlotRequest { Name = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
        [Test]
        public void Validate_IdIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UpdateServerSlotRequest { Id = null, Name = "fff" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
        [Test]
        public void Validate_IdTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UpdateServerSlotRequest { Id = new string('A', 257), Name = "fff" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
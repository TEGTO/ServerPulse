using FluentValidation.TestHelper;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Validators;

namespace ServerSlotApiTests.Validators
{
    [TestFixture]
    internal class CreateServerSlotRequestValidatorTests
    {
        private CreateServerSlotRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CreateServerSlotRequestValidator();
        }

        [Test]
        public void Validate_NameIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CreateServerSlotRequest { Name = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
        [Test]
        public void Validate_NameTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CreateServerSlotRequest { Name = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}
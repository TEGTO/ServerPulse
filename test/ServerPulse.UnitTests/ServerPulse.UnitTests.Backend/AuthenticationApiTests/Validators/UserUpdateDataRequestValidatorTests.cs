using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class UserUpdateDataRequestValidatorTests
    {
        private UserUpdateDataRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserUpdateDataRequestValidator();
        }

        [Test]
        public void Validate_UserNameIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { UserName = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
        [Test]
        public void Validate_UserNameTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { UserName = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
        [Test]
        public void Validate_OldEmailIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldEmail = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldEmail);
        }
        [Test]
        public void Validate_OldEmailWrongFormat_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldEmail = "123456" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldEmail);
        }
        [Test]
        public void Validate_OldEmailTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldEmail = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldEmail);
        }
        [Test]
        public void Validate_NewEmailWrongFormat_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { NewEmail = "123456" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewEmail);
        }
        [Test]
        public void Validate_NewEmailTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { NewEmail = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewEmail);
        }
        [Test]
        public void Validate_OldPasswordIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldPassword = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldPassword);
        }
        [Test]
        public void Validate_OldPasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldPassword = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldPassword);
        }
        [Test]
        public void Validate_OldPasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { OldPassword = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OldPassword);
        }
        [Test]
        public void Validate_NewPasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { NewPassword = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }
        [Test]
        public void Validate_NewPasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserUpdateDataRequest { NewPassword = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.NewPassword);
        }
    }
}

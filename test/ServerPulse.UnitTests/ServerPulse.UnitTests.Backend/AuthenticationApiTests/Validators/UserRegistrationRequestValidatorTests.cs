using AuthenticationApi.Domain.Dtos;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class UserRegistrationRequestValidatorTests
    {
        private UserRegistrationRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserRegistrationRequestValidator();
        }

        [Test]
        public void Validate_UserNameIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { UserName = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
        [Test]
        public void Validate_UserNameTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { UserName = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
        [Test]
        public void Validate_EmailIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Email = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
        [Test]
        public void Validate_EmailWrongFormat_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Email = "123456" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
        [Test]
        public void Validate_EmailTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Email = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
        [Test]
        public void Validate_PasswordIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Password = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Password = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Password = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_ConfirmPasswordIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { UserName = "UserName", Email = "example@smt.com", Password = "12345678", ConfirmPassword = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        }
        [Test]
        public void Validate_ConfirmPasswordNotSameAsPassword_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { Password = "12345678", ConfirmPassword = "12345" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        }
        [Test]
        public void Validate_ConfirmPasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { ConfirmPassword = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        }
        [Test]
        public void Validate_ConfirmPasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserRegistrationRequest { ConfirmPassword = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
        }
    }
}

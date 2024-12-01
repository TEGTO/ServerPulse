using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;
using Shared.Dtos.Auth;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    public class CheckAuthDataRequestValidatorTests
    {
        private CheckAuthDataRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new CheckAuthDataRequestValidator();
        }

        [Test]
        public void Validate_LoginIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Login = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void Validate_LoginTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Login = new string('A', 257), Password = "12345678" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void Validate_PasswordIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Password = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Password = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new CheckAuthDataRequest { Password = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
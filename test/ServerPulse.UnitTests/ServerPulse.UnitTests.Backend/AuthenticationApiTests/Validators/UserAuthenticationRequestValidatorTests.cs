using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class UserAuthenticationRequestValidatorTests
    {
        private UserAuthenticationRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserAuthenticationRequestValidator();
        }

        [Test]
        public void Validate_LoginIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserAuthenticationRequest { Login = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void Validate_LoginTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserAuthenticationRequest { Login = new string('A', 257), Password = "12345678" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void Validate_PasswordIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserAuthenticationRequest { Password = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooSmall_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserAuthenticationRequest { Password = "1234" };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void Validate_PasswordTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new UserAuthenticationRequest { Password = new string('A', 257) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}

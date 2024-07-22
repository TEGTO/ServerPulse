using FluentValidation.TestHelper;
using Shared.Dtos.Auth;
using Shared.Validators;

namespace SharedTests.Validators
{
    [TestFixture]
    public class CheckAuthDataRequestValidatorTests
    {
        private CheckAuthDataRequestValidator validator;

        [SetUp]
        public void Setup()
        {
            validator = new CheckAuthDataRequestValidator();
        }

        [Test]
        public void ValidateLogin_LoginIsNull_ValidationError()
        {
            // Arrange
            var model = new CheckAuthDataRequest { Login = null, Password = "validPassword" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void ValidateLogin_LoginIsEmpty_ValidationError()
        {
            // Arrange
            var model = new CheckAuthDataRequest { Login = string.Empty, Password = "validPassword" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void ValidateLogin_LoginIsTooLong_ValidationError()
        {
            // Arrange
            var longLogin = new string('x', 257);
            var model = new CheckAuthDataRequest { Login = longLogin, Password = "validPassword" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Login);
        }
        [Test]
        public void ValidatePassword_PasswordIsNull_ValidationError()
        {
            // Arrange
            var model = new CheckAuthDataRequest { Login = "validLogin", Password = null };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void ValidatePassword_PasswordIsEmpty_ValidationError()
        {
            // Arrange
            var model = new CheckAuthDataRequest { Login = "validLogin", Password = string.Empty };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void ValidatePassword_PasswordIsTooLong_ValidationError()
        {
            // Arrange
            var longPassword = new string('x', 257);
            var model = new CheckAuthDataRequest { Login = "validLogin", Password = longPassword };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
        [Test]
        public void ValidateRequest_LoginAndPasswordAreValid_NoValidationError()
        {
            // Arrange
            var model = new CheckAuthDataRequest { Login = "validLogin", Password = "validPassword" };
            // Act
            var result = validator.TestValidate(model);
            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Login);
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }
    }
}
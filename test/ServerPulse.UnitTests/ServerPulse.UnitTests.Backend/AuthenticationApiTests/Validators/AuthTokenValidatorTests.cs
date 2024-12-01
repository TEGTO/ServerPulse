using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Infrastructure.Validators;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Validators.Tests
{
    public class AuthTokenValidatorTests
    {
        private AuthTokenValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new AuthTokenValidator();
        }

        [Test]
        public void Validate_AccessTokenIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new AuthToken { AccessToken = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }
        [Test]
        public void Validate_AuthTokenTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new AuthToken { AccessToken = new string('A', 2049) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }
        [Test]
        public void Validate_RefreshTokenIsNull_ShouldHaveValidationError()
        {
            // Arrange
            var request = new AuthToken { RefreshToken = null };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }
        [Test]
        public void Validate_RefreshTokenTooBig_ShouldHaveValidationError()
        {
            // Arrange
            var request = new AuthToken { RefreshToken = new string('A', 2049) };
            // Act
            var result = validator.TestValidate(request);
            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }
    }
}
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth;
using FluentValidation.TestHelper;

namespace UserApi.Validators.Tests
{
    [TestFixture]
    internal class LoginOAuthRequestValidatorTests
    {
        private LoginOAuthRequestValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new LoginOAuthRequestValidator();
        }

        [Test]
        public void Should_HaveError_When_CodeIsNull()
        {
            var model = new LoginOAuthRequest { Code = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Test]
        public void Should_HaveError_When_CodeIsEmpty()
        {
            var model = new LoginOAuthRequest { Code = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Test]
        public void Should_HaveError_When_CodeExceedsMaxLength()
        {
            var model = new LoginOAuthRequest { Code = new string('a', 1025) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierIsNull()
        {
            var model = new LoginOAuthRequest { CodeVerifier = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierIsEmpty()
        {
            var model = new LoginOAuthRequest { CodeVerifier = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierExceedsMaxLength()
        {
            var model = new LoginOAuthRequest { CodeVerifier = new string('b', 1025) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsNull()
        {
            var model = new LoginOAuthRequest { RedirectUrl = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsEmpty()
        {
            var model = new LoginOAuthRequest { RedirectUrl = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlExceedsMaxLength()
        {
            var model = new LoginOAuthRequest { RedirectUrl = new string('c', 1025) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_NotHaveError_When_AllFieldsAreValid()
        {
            var model = new LoginOAuthRequest
            {
                Code = "validCode",
                CodeVerifier = "validCodeVerifier",
                RedirectUrl = "http://valid-redirect-url.com",
                OAuthLoginProvider = OAuthLoginProvider.Google
            };

            var result = validator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.Code);
            result.ShouldNotHaveValidationErrorFor(x => x.CodeVerifier);
            result.ShouldNotHaveValidationErrorFor(x => x.RedirectUrl);
        }
    }
}
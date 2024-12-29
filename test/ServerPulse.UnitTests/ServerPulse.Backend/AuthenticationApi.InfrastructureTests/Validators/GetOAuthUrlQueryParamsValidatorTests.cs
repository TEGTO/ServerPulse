using AuthenticationApi.Dtos.OAuth;
using FluentValidation.TestHelper;
using UserApi.Validators;

namespace AuthenticationApi.Validators.Tests
{
    [TestFixture]
    internal class GetOAuthUrlQueryParamsValidatorTests
    {
        private GetOAuthUrlQueryParamsValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new GetOAuthUrlQueryParamsValidator();
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsNull()
        {
            var model = new GetOAuthUrlQueryParams { RedirectUrl = null!, CodeVerifier = "validCodeVerifier" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsEmpty()
        {
            var model = new GetOAuthUrlQueryParams { RedirectUrl = "", CodeVerifier = "validCodeVerifier" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlExceedsMaxLength()
        {
            var model = new GetOAuthUrlQueryParams
            {
                RedirectUrl = new string('a', 1025),
                CodeVerifier = "validCodeVerifier"
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierIsNull()
        {
            var model = new GetOAuthUrlQueryParams { RedirectUrl = "http://valid-url.com", CodeVerifier = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierIsEmpty()
        {
            var model = new GetOAuthUrlQueryParams { RedirectUrl = "http://valid-url.com", CodeVerifier = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierExceedsMaxLength()
        {
            var model = new GetOAuthUrlQueryParams
            {
                RedirectUrl = "http://valid-url.com",
                CodeVerifier = new string('b', 1025)
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_NotHaveError_When_AllFieldsAreValid()
        {
            var model = new GetOAuthUrlQueryParams
            {
                RedirectUrl = "http://valid-url.com",
                CodeVerifier = "validCodeVerifier"
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.RedirectUrl);
            result.ShouldNotHaveValidationErrorFor(x => x.CodeVerifier);
        }
    }
}
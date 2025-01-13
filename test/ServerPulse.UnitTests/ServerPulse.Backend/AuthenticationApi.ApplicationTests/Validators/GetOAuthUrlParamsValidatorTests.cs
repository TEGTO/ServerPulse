using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Application.Validators.Tests
{
    [TestFixture]
    internal class GetOAuthUrlParamsValidatorTests
    {
        private GetOAuthUrlParamsValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new GetOAuthUrlParamsValidator();
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsNull()
        {
            var model = new GetOAuthUrlParams { RedirectUrl = null!, CodeVerifier = "validCodeVerifier" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsEmpty()
        {
            var model = new GetOAuthUrlParams { RedirectUrl = "", CodeVerifier = "validCodeVerifier" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlExceedsMaxLength()
        {
            var model = new GetOAuthUrlParams
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
            var model = new GetOAuthUrlParams { RedirectUrl = "http://valid-url.com", CodeVerifier = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierIsEmpty()
        {
            var model = new GetOAuthUrlParams { RedirectUrl = "http://valid-url.com", CodeVerifier = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CodeVerifier);
        }

        [Test]
        public void Should_HaveError_When_CodeVerifierExceedsMaxLength()
        {
            var model = new GetOAuthUrlParams
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
            var model = new GetOAuthUrlParams
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
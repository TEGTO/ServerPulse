using AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Core.Enums;
using FluentValidation.TestHelper;

namespace AuthenticationApi.Application.Validators.Tests
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
        public void Should_HaveError_When_QueryParamsIsNull()
        {
            var model = new LoginOAuthRequest { QueryParams = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.QueryParams);
        }

        [Test]
        public void Should_HaveError_When_QueryParamsIsEmpty()
        {
            var model = new LoginOAuthRequest { QueryParams = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.QueryParams);
        }

        [Test]
        public void Should_HaveError_When_QueryParamsExceedsMaxLength()
        {
            var model = new LoginOAuthRequest { QueryParams = new string('a', 1025) };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.QueryParams);
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
                QueryParams = "params",
                RedirectUrl = "http://valid-redirect-url.com",
                OAuthLoginProvider = OAuthLoginProvider.Google
            };

            var result = validator.TestValidate(model);

            result.ShouldNotHaveValidationErrorFor(x => x.QueryParams);
            result.ShouldNotHaveValidationErrorFor(x => x.RedirectUrl);
        }
    }
}
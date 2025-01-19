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
            var model = new GetOAuthUrlParams { RedirectUrl = null! };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlIsEmpty()
        {
            var model = new GetOAuthUrlParams { RedirectUrl = "" };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_HaveError_When_RedirectUrlExceedsMaxLength()
        {
            var model = new GetOAuthUrlParams
            {
                RedirectUrl = new string('a', 1025),
            };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RedirectUrl);
        }

        [Test]
        public void Should_NotHaveError_When_AllFieldsAreValid()
        {
            var model = new GetOAuthUrlParams
            {
                RedirectUrl = "http://valid-url.com",
            };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.RedirectUrl);
        }
    }
}
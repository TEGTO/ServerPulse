using AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl;
using FluentValidation;

namespace AuthenticationApi.Application.Validators
{
    public class GetOAuthUrlParamsValidator : AbstractValidator<GetOAuthUrlParams>
    {
        public GetOAuthUrlParamsValidator()
        {
            RuleFor(x => x.RedirectUrl).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.CodeVerifier).NotNull().NotEmpty().MaximumLength(1024);
        }
    }
}

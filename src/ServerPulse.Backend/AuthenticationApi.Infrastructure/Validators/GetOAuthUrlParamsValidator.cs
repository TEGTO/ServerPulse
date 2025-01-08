using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.GetOAuthUrl;
using FluentValidation;

namespace UserApi.Validators
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

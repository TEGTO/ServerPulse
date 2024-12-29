using AuthenticationApi.Dtos.OAuth;
using FluentValidation;

namespace UserApi.Validators
{
    public class GetOAuthUrlQueryParamsValidator : AbstractValidator<GetOAuthUrlQueryParams>
    {
        public GetOAuthUrlQueryParamsValidator()
        {
            RuleFor(x => x.RedirectUrl).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.CodeVerifier).NotNull().NotEmpty().MaximumLength(1024);
        }
    }
}

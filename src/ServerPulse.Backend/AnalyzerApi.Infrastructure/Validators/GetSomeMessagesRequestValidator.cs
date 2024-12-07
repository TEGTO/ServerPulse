using AnalyzerApi.Infrastructure.Requests;
using FluentValidation;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class GetSomeMessagesRequestValidator : AbstractValidator<GetSomeMessagesRequest>
    {
        public GetSomeMessagesRequestValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.NumberOfMessages).GreaterThan(0).LessThanOrEqualTo(50);
        }
    }
}
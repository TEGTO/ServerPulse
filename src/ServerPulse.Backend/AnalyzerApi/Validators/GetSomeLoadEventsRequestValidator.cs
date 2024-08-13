using AnalyzerApi.Domain.Dtos.Requests;
using FluentValidation;

namespace AnalyzerApi.Validators
{
    public class GetSomeLoadEventsRequestValidator : AbstractValidator<GetSomeLoadEventsRequest>
    {
        public GetSomeLoadEventsRequestValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.NumberOfMessages).GreaterThan(0);
        }
    }
}
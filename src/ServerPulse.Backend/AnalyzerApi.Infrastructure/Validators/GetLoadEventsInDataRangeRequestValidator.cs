using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetLoadEventsInDataRange;
using FluentValidation;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class GetLoadEventsInDataRangeRequestValidator : AbstractValidator<GetLoadEventsInDataRangeRequest>
    {
        public GetLoadEventsInDataRangeRequestValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
        }
    }
}
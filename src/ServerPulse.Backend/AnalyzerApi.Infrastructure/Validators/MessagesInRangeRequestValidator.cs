using AnalyzerApi.Infrastructure.Requests;
using FluentValidation;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class MessagesInRangeRequestValidator : AbstractValidator<MessagesInRangeRequest>
    {
        public MessagesInRangeRequestValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
        }
    }
}
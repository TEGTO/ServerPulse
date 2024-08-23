using AnalyzerApi.Domain.Dtos.Requests;
using FluentValidation;

namespace AnalyzerApi.Validators
{
    public class MessagesInRangeRangeRequestValidator : AbstractValidator<MessagesInRangeRangeRequest>
    {
        public MessagesInRangeRangeRequestValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
        }
    }
}
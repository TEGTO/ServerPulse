using AnalyzerApi.Domain.Dtos.Requests;
using FluentValidation;

namespace AnalyzerApi.Validators
{
    public class MessageAmountInRangeRequestValidator : AbstractValidator<MessageAmountInRangeRequest>
    {
        public MessageAmountInRangeRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
            RuleFor(x => x.TimeSpan).GreaterThan(TimeSpan.FromSeconds(int.Parse(configuration[Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS]!)));
        }
    }
}

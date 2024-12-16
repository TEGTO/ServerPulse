using AnalyzerApi.Infrastructure.Requests;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class MessageAmountInRangeRequestValidator : AbstractValidator<MessageAmountInRangeRequest>
    {
        public MessageAmountInRangeRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
            RuleFor(x => x.TimeSpan).GreaterThanOrEqualTo(TimeSpan.FromSeconds(int.Parse(configuration[Configuration.MIN_STATISTICS_TIMESPAN_IN_SECONDS]!)));
        }
    }
}

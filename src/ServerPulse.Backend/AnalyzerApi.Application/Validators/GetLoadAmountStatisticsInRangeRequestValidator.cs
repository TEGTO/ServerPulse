using AnalyzerApi.Core.Dtos.Endpoints.Analyze.GetLoadAmountStatisticsInRange;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace AnalyzerApi.Application.Validators
{
    public class GetLoadAmountStatisticsInRangeRequestValidator : AbstractValidator<GetLoadAmountStatisticsInRangeRequest>
    {
        public GetLoadAmountStatisticsInRangeRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.From).LessThan(x => x.To);
            RuleFor(x => x.To).GreaterThan(x => x.From);
            RuleFor(x => x.TimeSpan).GreaterThanOrEqualTo(TimeSpan.FromSeconds(int.Parse(configuration[ConfigurationKeys.MIN_STATISTICS_TIMESPAN_IN_SECONDS]!)));
        }
    }
}

using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Dtos.Endpoints.Analyze.GetSomeCustomEvents;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class GetSomeCustomEventsRequestValidator : AbstractValidator<GetSomeCustomEventsRequest>
    {
        public GetSomeCustomEventsRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.NumberOfMessages).GreaterThan(0).LessThanOrEqualTo(int.Parse(configuration[ConfigurationKeys.MAX_EVENT_AMOUNT_PER_REQUEST]!));
        }
    }
}

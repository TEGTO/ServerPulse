using AnalyzerApi.Infrastructure.Requests;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace AnalyzerApi.Infrastructure.Validators
{
    public class GetSomeMessagesRequestValidator : AbstractValidator<GetSomeMessagesRequest>
    {
        public GetSomeMessagesRequestValidator(IConfiguration configuration)
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.NumberOfMessages).GreaterThan(0).LessThanOrEqualTo(int.Parse(configuration[Configuration.MAX_EVENT_AMOUNT_PER_REQUEST]!));
        }
    }
}
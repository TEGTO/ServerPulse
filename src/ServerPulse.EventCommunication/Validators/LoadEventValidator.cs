using FluentValidation;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunication.Validators
{
    public class LoadEventValidator : AbstractValidator<LoadEvent>
    {
        public LoadEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Method).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.StatusCode).GreaterThanOrEqualTo(0);
        }
    }
}
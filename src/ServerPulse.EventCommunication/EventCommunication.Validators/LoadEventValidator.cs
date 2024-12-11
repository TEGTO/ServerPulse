using EventCommunication.Events;
using FluentValidation;

namespace EventCommunication.Validators
{
    public class LoadEventValidator : BaseEventValidator<LoadEvent>
    {
        public LoadEventValidator() : base()
        {
            RuleFor(x => x.Endpoint).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Method).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.StatusCode).GreaterThanOrEqualTo(0);
        }
    }
}
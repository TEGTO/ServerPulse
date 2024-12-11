using EventCommunication.Wrappers;
using FluentValidation;

namespace EventCommunication.Validators
{
    public class CustomEventWrapperValidator : AbstractValidator<CustomEventWrapper>
    {
        public CustomEventWrapperValidator()
        {
            RuleFor(x => x.CustomEvent).NotNull();
            RuleFor(x => x.CustomEventSerialized).NotNull().NotEmpty();
        }
    }
}
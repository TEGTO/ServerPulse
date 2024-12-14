using FluentValidation;

namespace EventCommunication.Validators
{
    public class CustomEventContainerValidator : AbstractValidator<CustomEventContainer>
    {
        public CustomEventContainerValidator()
        {
            RuleFor(x => x.CustomEvent).NotNull();
            RuleFor(x => x.CustomEventSerialized).NotNull().NotEmpty();
        }
    }
}
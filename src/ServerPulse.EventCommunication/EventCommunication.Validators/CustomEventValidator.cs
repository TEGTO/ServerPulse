using FluentValidation;

namespace EventCommunication.Validators
{
    public class CustomEventValidator : BaseEventValidator<CustomEvent>
    {
        public CustomEventValidator() : base()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(512);
            RuleFor(x => x.Description).NotNull().NotEmpty().MaximumLength(1024);
        }
    }
}

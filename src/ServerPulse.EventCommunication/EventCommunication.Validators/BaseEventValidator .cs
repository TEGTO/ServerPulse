using FluentValidation;

namespace EventCommunication.Validators
{
    public class BaseEventValidator<T> : AbstractValidator<T> where T : BaseEvent
    {
        public BaseEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
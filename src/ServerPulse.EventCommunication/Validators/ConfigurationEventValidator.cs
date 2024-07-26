using FluentValidation;
using ServerPulse.EventCommunication.Events;

namespace ServerPulse.EventCommunication.Validators
{
    public class ConfigurationEventValidator : AbstractValidator<ConfigurationEvent>
    {
        public ConfigurationEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
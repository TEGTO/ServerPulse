using FluentValidation;

namespace EventCommunication.Validators
{
    public class ConfigurationEventValidator : BaseEventValidator<ConfigurationEvent>
    {
        public ConfigurationEventValidator() : base()
        {
            RuleFor(x => x.ServerKeepAliveInterval).GreaterThanOrEqualTo(TimeSpan.Zero);
        }
    }
}
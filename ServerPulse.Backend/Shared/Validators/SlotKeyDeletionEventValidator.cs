using FluentValidation;
using Shared.Dtos.ServerEvent;

namespace Shared.Validators
{
    public class SlotKeyDeletionEventValidator : AbstractValidator<SlotKeyDeletionEvent>
    {
        public SlotKeyDeletionEventValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}

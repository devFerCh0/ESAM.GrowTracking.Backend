using ESAM.GrowTracking.Application.Commons.Validators;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
    {
        public RefreshCommandValidator()
        {
            RuleFor(rc => rc.RefreshTokenRaw).Cascade(CascadeMode.Stop)
                .MinimumLength(3).WithMessage(MessagesValidator.RefreshTokenMinLength)
                .MaximumLength(256).WithMessage(MessagesValidator.RefreshTokenMaxLength)
                .When(rc => !string.IsNullOrWhiteSpace(rc.RefreshTokenRaw));
            RuleFor(rc => rc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(MessagesValidator.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(MessagesValidator.DeviceIdentifierMaxLength)
                .Must(UtilityValidator.IsValidGuid).WithMessage(MessagesValidator.DeviceIdentifierInvalid);
        }
    }
}
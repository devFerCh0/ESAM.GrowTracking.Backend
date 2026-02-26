using ESAM.GrowTracking.Application.Commons.Validators;
using ESAM.GrowTracking.Domain.Catalogs;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(lc => lc.Credential).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.CredentialRequired)
                .MinimumLength(5).WithMessage(MessagesValidator.CredentialMinLength)
                .MaximumLength(50).WithMessage(MessagesValidator.CredentialMaxLength)
                .Must(UtilityValidator.IsValidCredential).WithMessage((alc, id) => id.Contains('@') ? MessagesValidator.CredentialValidatedEmail : MessagesValidator.CredentialValidatedUsername);                     
            RuleFor(lc => lc.Password).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.PasswordRequired)
                .MinimumLength(5).WithMessage(MessagesValidator.PasswordMinLength)
                .MaximumLength(100).WithMessage(MessagesValidator.PasswordMaxLength);
            RuleFor(lc => lc.IsPersistent).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(MessagesValidator.IsPersistentRequired);
            RuleFor(lc => lc.DeviceIdentifier).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.DeviceIdentifierRequired)
                .MinimumLength(3).WithMessage(MessagesValidator.DeviceIdentifierMinLength)
                .MaximumLength(256).WithMessage(MessagesValidator.DeviceIdentifierMaxLength)
                .Must(UtilityValidator.IsValidGuid).WithMessage(MessagesValidator.DeviceIdentifierInvalid);
            RuleFor(lc => lc.DeviceName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.DeviceNameRequired)
                .MinimumLength(2).WithMessage(MessagesValidator.DeviceNameMinLength)
                .MaximumLength(100).WithMessage(MessagesValidator.DeviceNameMaxLength)
                .Must(UtilityValidator.ContainsControlChars).WithMessage(MessagesValidator.DeviceNameInvalid);
            RuleFor(lc => lc.ApiClientType).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.ApiClientTypeRequired)
                .Must(UtilityValidator.BeAValidEnum<ApiClientType>).WithMessage(MessagesValidator.ApiClientTypeInvalid);
        }
    }
}
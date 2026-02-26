using ESAM.GrowTracking.Application.Commons.Validators;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole
{
    public class LoginAssumedRoleCommandValidator : AbstractValidator<LoginAssumedRoleCommand>
    {
        public LoginAssumedRoleCommandValidator()
        {
            RuleFor(lwrac => lwrac.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(MessagesValidator.WorkProfileIdInvalid);
            RuleFor(lwrac => lwrac.RoleId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.RoleIdRequired)
                .GreaterThan(0).WithMessage(MessagesValidator.RoleIdInvalid);
            RuleFor(lwrac => lwrac.CampusId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.CampusIdRequired)
                .GreaterThan(0).WithMessage(MessagesValidator.CampusIdInvalid);
        }
    }
}
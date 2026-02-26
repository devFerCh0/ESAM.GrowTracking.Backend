using ESAM.GrowTracking.Application.Commons.Validators;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile
{
    public class LoginAssumedWorkProfileCommandValidator : AbstractValidator<LoginAssumedWorkProfileCommand>
    {
        public LoginAssumedWorkProfileCommandValidator()
        {
            RuleFor(lwrac => lwrac.WorkProfileId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage(MessagesValidator.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(MessagesValidator.WorkProfileIdInvalid);
        }
    }
}
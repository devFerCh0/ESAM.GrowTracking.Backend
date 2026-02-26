using ESAM.GrowTracking.Application.Commons.Validators;
using FluentValidation;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses
{
    public class LoginUserRoleCampusQueryValidator : AbstractValidator<LoginUserRoleCampusQuery>
    {
        public LoginUserRoleCampusQueryValidator()
        {
            RuleFor(lurcq => lurcq.WorkProfileId).Cascade(CascadeMode.Stop).NotEmpty().WithMessage(MessagesValidator.WorkProfileIdRequired)
                .GreaterThan(0).WithMessage(MessagesValidator.WorkProfileIdInvalid);
        }
    }
}
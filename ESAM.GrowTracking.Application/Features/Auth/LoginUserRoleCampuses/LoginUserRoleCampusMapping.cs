using AutoMapper;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses
{
    public class LoginUserRoleCampusMapping : Profile
    {
        public LoginUserRoleCampusMapping()
        {
            CreateMap<LoginUserRoleCampusProjection, LoginUserRoleCampusReadModel>();
        }
    }
}
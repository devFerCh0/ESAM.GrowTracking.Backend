using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.LoginUserRoleCampuses;

namespace ESAM.GrowTracking.API.Controllers.Auth.LoginUserRoleCampuses
{
    public class LoginUserRoleCampusMapping : Profile
    {
        public LoginUserRoleCampusMapping()
        {
            CreateMap<LoginUserRoleCampusReadModel, LoginUserRoleCampusResponse>();
        }
    }
}
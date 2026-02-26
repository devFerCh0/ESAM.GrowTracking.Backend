using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.Logout;

namespace ESAM.GrowTracking.API.Controllers.Auth.Logout
{
    public class LogoutMapping : Profile
    {
        public LogoutMapping()
        {
            CreateMap<LogoutRequest, LogoutCommand>();
        }
    }
}
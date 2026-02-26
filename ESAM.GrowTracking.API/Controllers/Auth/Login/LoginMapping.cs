using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.Login.Responses;
using ESAM.GrowTracking.Application.Features.Auth.Login;
using ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels;

namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public class LoginMapping : Profile
    {
        public LoginMapping()
        {
            CreateMap<LoginRequest, LoginCommand>();
            CreateMap<LoginUserWorkProfileReadModel, LoginUserWorkProfileResponse>();
            CreateMap<LoginUserReadModel, LoginUserResponse>();
            CreateMap<LoginReadModel, LoginResponse>();
        }
    }
}
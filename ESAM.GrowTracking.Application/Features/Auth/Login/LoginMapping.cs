using AutoMapper;
using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Features.Auth.Login.Projections;
using ESAM.GrowTracking.Application.Features.Auth.Login.ReadModels;

namespace ESAM.GrowTracking.Application.Features.Auth.Login
{
    public class LoginMapping : Profile
    {
        public LoginMapping()
        {
            CreateMap<LoginUserWorkProfileProjection, LoginUserWorkProfileReadModel>();
            CreateMap<LoginUserProjection, LoginUserReadModel>();
            CreateMap<(AccessTokenDTO accessToken, LoginUserProjection loginUser), LoginReadModel>()
                .ForCtorParam("accessToken", opt => opt.MapFrom(src => src.accessToken.AccessToken))
                .ForCtorParam("accessTokenExpiresIn", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresIn))
                .ForCtorParam("accessTokenExpiresAt", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresAt))
                .ForCtorParam("loginUser", opt => opt.MapFrom(src => src.loginUser));
        }
    }
}
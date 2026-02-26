using AutoMapper;
using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile
{
    public class LoginAssumedWorkProfileMapping : Profile
    {
        public LoginAssumedWorkProfileMapping()
        {
            CreateMap<LoginAssumedWorkProfileUserWorkProfileProjection, LoginAssumedWorkProfileUserWorkProfileReadModel>();
            CreateMap<LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedProjection, LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel>();
            CreateMap<LoginAssumedWorkProfileUserSessionProjection, LoginAssumedWorkProfileUserSessionReadModel>();
            CreateMap<LoginAssumedWorkProfileUserProjection, LoginAssumedWorkProfileUserReadModel>();
            CreateMap<(AccessTokenDTO accessToken, RefreshTokenDTO refreshToken, LoginAssumedWorkProfileUserProjection loginAssumedWorkProfileUser), LoginAssumedWorkProfileReadModel>()
                .ForCtorParam("accessToken", opt => opt.MapFrom(src => src.accessToken.AccessToken))
                .ForCtorParam("accessTokenExpiresIn", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresIn))
                .ForCtorParam("accessTokenExpiresAt", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresAt))
                .ForCtorParam("tokenIdentifier", opt => opt.MapFrom(src => src.refreshToken.TokenIdentifier))
                .ForCtorParam("refreshTokenPlain", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenPlain))
                .ForCtorParam("refreshTokenExpiresIn", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresIn))
                .ForCtorParam("refreshTokenExpiresAt", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresAt))
                .ForCtorParam("loginAssumedWorkProfileUser", opt => opt.MapFrom(src => src.loginAssumedWorkProfileUser));
        }
    }
}
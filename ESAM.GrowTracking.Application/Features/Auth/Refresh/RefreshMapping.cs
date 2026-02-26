using AutoMapper;
using ESAM.GrowTracking.Application.Commons.DTOs;

namespace ESAM.GrowTracking.Application.Features.Auth.Refresh
{
    public class RefreshMapping : Profile
    {
        public RefreshMapping()
        {
            CreateMap<(AccessTokenDTO accessToken, RefreshTokenDTO refreshToken), RefreshReadModel>()
                .ForCtorParam("accessToken", opt => opt.MapFrom(src => src.accessToken.AccessToken))
                .ForCtorParam("accessTokenExpiresIn", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresIn))
                .ForCtorParam("accessTokenExpiresAt", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresAt))
                .ForCtorParam("tokenIdentifier", opt => opt.MapFrom(src => src.refreshToken.TokenIdentifier))
                .ForCtorParam("refreshTokenPlain", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenPlain))
                .ForCtorParam("refreshTokenExpiresIn", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresIn))
                .ForCtorParam("refreshTokenExpiresAt", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresAt));
        }
    }
}
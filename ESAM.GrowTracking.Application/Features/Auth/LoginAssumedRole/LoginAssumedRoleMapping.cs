using AutoMapper;
using ESAM.GrowTracking.Application.Commons.DTOs;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.Projections;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels;

namespace ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole
{
    public class LoginAssumedRoleMapping : Profile
    {
        public LoginAssumedRoleMapping()
        {
            CreateMap<LoginAssumedRoleUserWorkProfileProjection, LoginAssumedRoleUserWorkProfileReadModel>();
            CreateMap<LoginAssumedRoleUserRoleCampusProjection, LoginAssumedRoleUserRoleCampusReadModel>();
            CreateMap<LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedProjection, LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel>();
            CreateMap<LoginAssumedRoleUserSessionUserWorkProfileSelectedProjection, LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel>();
            CreateMap<LoginAssumedRoleUserSessionProjection, LoginAssumedRoleUserSessionReadModel>();
            CreateMap<LoginAssumedRoleUserProjection, LoginAssumedRoleUserReadModel>();
            CreateMap<(AccessTokenDTO accessToken, RefreshTokenDTO refreshToken, LoginAssumedRoleUserProjection loginAssumedRoleUser), LoginAssumedRoleReadModel>()
                .ForCtorParam("accessToken", opt => opt.MapFrom(src => src.accessToken.AccessToken))
                .ForCtorParam("accessTokenExpiresIn", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresIn))
                .ForCtorParam("accessTokenExpiresAt", opt => opt.MapFrom(src => src.accessToken.AccessTokenExpiresAt))
                .ForCtorParam("tokenIdentifier", opt => opt.MapFrom(src => src.refreshToken.TokenIdentifier))
                .ForCtorParam("refreshTokenPlain", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenPlain))
                .ForCtorParam("refreshTokenExpiresIn", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresIn))
                .ForCtorParam("refreshTokenExpiresAt", opt => opt.MapFrom(src => src.refreshToken.RefreshTokenExpiresAt))
                .ForCtorParam("loginAssumedRoleUser", opt => opt.MapFrom(src => src.loginAssumedRoleUser));
        }
    }
}
using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole.Responses;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedRole.ReadModels;

namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedRole
{
    public class LoginAssumedRoleMapping : Profile
    {
        public LoginAssumedRoleMapping()
        {
            CreateMap<LoginAssumedRoleRequest, LoginAssumedRoleCommand>();
            CreateMap<LoginAssumedRoleUserWorkProfileReadModel, LoginAssumedRoleUserWorkProfileResponse>();
            CreateMap<LoginAssumedRoleUserRoleCampusReadModel, LoginAssumedRoleUserRoleCampusResponse>();
            CreateMap<LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedReadModel, LoginAssumedRoleUserSessionUserWorkProfileSelectedUserRoleCampusSelectedResponse>();
            CreateMap<LoginAssumedRoleUserSessionUserWorkProfileSelectedReadModel, LoginAssumedRoleUserSessionUserWorkProfileSelectedResponse>();
            CreateMap<LoginAssumedRoleUserSessionReadModel, LoginAssumedRoleUserSessionResponse>();
            CreateMap<LoginAssumedRoleUserReadModel, LoginAssumedRoleUserResponse>();
            CreateMap<LoginAssumedRoleReadModel, LoginAssumedRoleResponse>().ForMember(dest => dest.RefreshTokenRaw, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (context.Items.TryGetValue("IsBrowser", out var isBrowser) && (bool)isBrowser)
                    return string.Empty;
                return src.RefreshTokenRaw;
            }));
        }
    }
}
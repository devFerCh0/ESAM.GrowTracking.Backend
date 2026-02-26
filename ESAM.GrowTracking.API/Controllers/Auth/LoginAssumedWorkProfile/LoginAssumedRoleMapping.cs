using AutoMapper;
using ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile.Responses;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile;
using ESAM.GrowTracking.Application.Features.Auth.LoginAssumedWorkProfile.ReadModels;

namespace ESAM.GrowTracking.API.Controllers.Auth.LoginAssumedWorkProfile
{
    public class LoginAssumedWorkProfileMapping : Profile
    {
        public LoginAssumedWorkProfileMapping()
        {
            CreateMap<LoginAssumedWorkProfileRequest, LoginAssumedWorkProfileCommand>();
            CreateMap<LoginAssumedWorkProfileUserWorkProfileReadModel, LoginAssumedWorkProfileUserWorkProfileResponse>();
            CreateMap<LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedReadModel, LoginAssumedWorkProfileUserSessionUserWorkProfileSelectedResponse>();
            CreateMap<LoginAssumedWorkProfileUserSessionReadModel, LoginAssumedWorkProfileUserSessionResponse>();
            CreateMap<LoginAssumedWorkProfileUserReadModel, LoginAssumedWorkProfileUserResponse>();
            CreateMap<LoginAssumedWorkProfileReadModel, LoginAssumedWorkProfileResponse>().ForMember(dest => dest.RefreshTokenRaw, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (context.Items.TryGetValue("IsBrowser", out var isBrowser) && (bool)isBrowser)
                    return string.Empty;
                return src.RefreshTokenRaw;
            }));
        }
    }
}
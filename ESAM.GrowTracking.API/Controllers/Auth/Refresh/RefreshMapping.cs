using AutoMapper;
using ESAM.GrowTracking.Application.Features.Auth.Refresh;

namespace ESAM.GrowTracking.API.Controllers.Auth.Refresh
{
    public class RefreshMapping : Profile
    {
        public RefreshMapping()
        {
            CreateMap<RefreshRequest, RefreshCommand>();
            CreateMap<RefreshReadModel, RefreshResponse>().ForMember(dest => dest.RefreshTokenRaw, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (context.Items.TryGetValue("IsBrowser", out var isBrowser) && (bool)isBrowser)
                    return string.Empty;
                return src.RefreshTokenRaw;
            }));
        }
    }
}
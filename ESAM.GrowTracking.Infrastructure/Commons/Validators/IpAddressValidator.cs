using ESAM.GrowTracking.Infrastructure.Commons.Extensions;
using System.Net;

namespace ESAM.GrowTracking.Infrastructure.Commons.Validators
{
    public class IpAddressValidator : IIpAddressValidator
    {
        public bool TryValidate(string candidate, out IPAddress? address)
        {
            address = null;
            if (!IPAddress.TryParse(candidate, out var parsed))
                return false;
            if (parsed.IsLoopback() || parsed.IsPrivate() || parsed.IsReserved())
                return false;
            address = parsed;
            return true;
        }
    }
}
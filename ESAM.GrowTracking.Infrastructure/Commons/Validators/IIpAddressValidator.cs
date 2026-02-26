using System.Net;

namespace ESAM.GrowTracking.Infrastructure.Commons.Validators
{
    public interface IIpAddressValidator
    {
        bool TryValidate(string candidate, out IPAddress? address);
    }
}
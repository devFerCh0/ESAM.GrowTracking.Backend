using System.Net;
using System.Net.Sockets;

namespace ESAM.GrowTracking.Infrastructure.Commons.Extensions
{
    public static class IpAddressExtension
    {
        public static bool IsPrivate(this IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                return b[0] == 10 || b[0] == 172 && b[1] >= 16 && b[1] <= 31 || b[0] == 192 && b[1] == 168 || b[0] == 100 && b[1] >= 64 && b[1] <= 127 || b[0] == 169 && b[1] == 254;
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var bytes = ip.GetAddressBytes();
                return (bytes[0] & 0xfe) == 0xfc || ip.IsIPv6LinkLocal;
            }
            return false;
        }

        public static bool IsLoopback(this IPAddress ip) => IPAddress.IsLoopback(ip);

        public static bool IsReserved(this IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                return b[0] == 192 && b[1] == 0 && b[2] == 2 || b[0] == 198 && b[1] == 51 && b[2] == 100 || b[0] == 203 && b[1] == 0 && b[2] == 113;
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var prefix = new IPAddress([0x20, 0x01, 0x0d, 0xb8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
                return ip.GetAddressBytes().AsSpan(0, 4).SequenceEqual(prefix.GetAddressBytes().AsSpan(0, 4));
            }
            return false;
        }
    }
}
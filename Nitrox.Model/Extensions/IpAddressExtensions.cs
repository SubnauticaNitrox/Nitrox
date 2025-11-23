using System;
using System.Net;
using System.Net.Sockets;

namespace Nitrox.Model.Extensions;

public static class IpAddressExtensions
{
    private static readonly string[] privateNetworks =
    [
        "10.0.0.0/8",
        "127.0.0.0/8",
        "172.16.0.0/12",
        "192.0.0.0/24 ",
        "192.168.0.0/16",
        "198.18.0.0/15"
    ];

    /// <summary>
    ///     Returns true if the given IP address is reserved for private networks.
    /// </summary>
    public static bool IsPrivate(this IPAddress address)
    {
        address = address.TryGetAsIPv4();
        switch (address.AddressFamily)
        {
            case AddressFamily.InterNetworkV6:
            {
                byte[] bytes = address.GetAddressBytes();
                // Unique local addresses (fc00::/7)
                if ((bytes[0] & 0xFE) == 0xFC)
                {
                    return true;
                }
                // Link-local addresses (fe80::/10)
                if (bytes[0] == 0xFE && (bytes[1] & 0xC0) == 0x80)
                {
                    return true;
                }
                break;
            }
            case AddressFamily.InterNetwork:
            {
                foreach (string privateSubnet in privateNetworks)
                {
                    if (IsInRange(address, privateSubnet))
                    {
                        return true;
                    }
                }
                break;
            }
        }
        return false;

        static bool IsInRange(IPAddress ipAddress, string mask)
        {
            string[] parts = mask.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(mask), "Mask must contain 2 parts: IP and host bits");
            }

            int ipNum = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
            int cidrAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
            int cidrMask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

            return (ipNum & cidrMask) == (cidrAddress & cidrMask);
        }
    }

    public static IPAddress TryGetAsIPv4(this IPAddress address)
    {
        if (address == null)
        {
            throw new ArgumentNullException(nameof(address));
        }
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }
}

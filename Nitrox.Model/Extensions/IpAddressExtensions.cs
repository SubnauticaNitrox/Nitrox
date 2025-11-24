using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Extensions;

public static class IpAddressExtensions
{
    /// <summary>
    ///     Returns true if the IP address points to the executing machine.
    /// </summary>
    public static bool IsLocalhost(this IPAddress? address)
    {
        if (address == null)
        {
            return false;
        }
        address = address.TryExtractMappedIPv4();
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }
        foreach (NetworkInterface ni in NetHelper.GetInternetInterfaces())
        {
            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (address.Equals(ip.Address.TryExtractMappedIPv4()))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    ///     Returns true if the given IP address is reserved for private networks.
    /// </summary>
    /// <remarks>
    ///     See reversed <a href="https://en.wikipedia.org/wiki/Reserved_IP_addresses#IPv4">IPv4</a> and
    ///     <a href="https://en.wikipedia.org/wiki/Reserved_IP_addresses#IPv6">IPv6</a> address ranges.
    /// </remarks>
    public static bool IsPrivate(this IPAddress address)
    {
        address = address.TryExtractMappedIPv4();
        Cidr[] privateSubnets = address.AddressFamily == AddressFamily.InterNetwork ? Cidr.PrivateIPv4Networks : Cidr.PrivateIPv6Networks;
        byte[] addressBytes = address.GetAddressBytes();
        foreach (Cidr privateSubnet in privateSubnets)
        {
            if (privateSubnet.ContainsHost(addressBytes))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///     Tries to get the IPv4 if it was mapped in another IP space, for example, IPv6.
    /// </summary>
    /// <remarks>
    ///     IPv6 address space supports the entire IPv4 address space using a reserved subspace:
    ///     <code>::ffff:0:0/96</code>
    ///     <b>First address:</b>
    ///     <code>
    ///      ::ffff:0.0.0.0
    ///      ::ffff:0:0
    ///      </code>
    ///     <b>Last address:</b>
    ///     <code>
    ///      ::ffff:255.255.255.255
    ///      ::ffff:ffff:ffff
    ///      </code>
    /// </remarks>
    public static IPAddress TryExtractMappedIPv4(this IPAddress address)
    {
        if (address == null)
        {
            throw new ArgumentNullException(nameof(address));
        }
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }

    /// <remarks>
    ///     See <a href="https://en.wikipedia.org/wiki/Classless_Inter-Domain_Routing">CIDR on Wikipedia</a>
    /// </remarks>
    private readonly record struct Cidr(byte[] NotationBytes, byte NetworkMaskBitSize)
    {
        /// <remarks>
        ///     See <a href="https://en.wikipedia.org/wiki/Reserved_IP_addresses#IPv4">all reserved IPv4 address ranges</a>.
        /// </remarks>
        public static readonly Cidr[] PrivateIPv4Networks =
        [
            "10.0.0.0/8",
            "127.0.0.0/8", // Loopback
            "172.16.0.0/12",
            "192.0.0.0/24 ",
            "192.168.0.0/16",
            "198.18.0.0/15"
        ];

        /// <remarks>
        ///     See <a href="https://en.wikipedia.org/wiki/Reserved_IP_addresses#IPv6">all reversed IPv6 address ranges</a>.
        /// </remarks>
        public static readonly Cidr[] PrivateIPv6Networks =
        [
            "::1/128", // Loopback
            "fc00::/7", // Unique local address
            "fe80::/10", // Link-local address
            "64:ff9b:1::/48" // Local-use IPv4/IPv6 translation
        ];

        public static implicit operator Cidr(string notation)
        {
            if (notation.LastIndexOf('/') is not (var slashIndex and >= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(notation));
            }
            if (!byte.TryParse(notation.Substring(slashIndex + 1), out byte networkMaskBitSize))
            {
                throw new ArgumentOutOfRangeException(nameof(notation), "CIDR network mask bit size is not a valid byte value");
            }
            byte[] notationBytes = IPAddress.Parse(notation.Substring(0, slashIndex)).GetAddressBytes();
            if (networkMaskBitSize > notationBytes.Length * 8)
            {
                throw new ArgumentOutOfRangeException(nameof(notation), "CIDR network mask bit size must not be more than total length of the CIDR notation");
            }
            return new Cidr(notationBytes, networkMaskBitSize);
        }

        /// <summary>
        ///     Returns true if the subnet defined by the CIDR notation could contain the host address.
        /// </summary>
        public bool ContainsHost(byte[] hostAddress)
        {
            // Compare the address with a CIDR mask, depending on the bit size.
            int hostAddressBitSize = hostAddress.Length * 8;
            switch (hostAddressBitSize)
            {
                case 32:
                {
                    int ipNum = BitConverter.ToInt32(hostAddress, 0);
                    int cidrNumber = BitConverter.ToInt32(NotationBytes, 0);
                    int cidrMask = IPAddress.HostToNetworkOrder(-1 << (hostAddressBitSize - NetworkMaskBitSize)); // -1 == all bits set, like uint.MaxValue
                    return (ipNum & cidrMask) == (cidrNumber & cidrMask);
                }
                default:
                {
                    byte[] networkBitMask = GetAllBitsSetArray(hostAddress.Length).BitwiseLeftShift(hostAddressBitSize - NetworkMaskBitSize).AsNetworkOrder();
                    return hostAddress.CreateCopy().BitwiseAnd(networkBitMask).SequenceEqual(NotationBytes.CreateCopy().BitwiseAnd(networkBitMask));
                }
            }
        }

        private static byte[] GetAllBitsSetArray(int byteSize) => Enumerable.Repeat(byte.MaxValue, byteSize).ToArray();
    }
}

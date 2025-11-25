using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
#if RELEASE
using System.Net.Http;
using System.Text.RegularExpressions;
#endif

namespace Nitrox.Model.Helper;

public static class NetHelper
{
    private static IPAddress? wanIpCache;
    private static IPAddress? lanIpCache;
    private static long lastSeenPacketChange = -1;
    private static long lastBytesSendOrReceived = -1;
    private static readonly object connectivityLock = new();
    private static readonly object wanIpLock = new();
    private static readonly object lanIpLock = new();

    private static bool? hasInternet;

    /// <summary>
    ///     Gets the network interfaces used for going onto the internet.
    ///     This is done by filtering for "Ethernet" and "Wi-Fi" network interfaces where "Ethernet" is returned earlier.
    /// </summary>
    /// <returns>Network interfaces used to go onto the internet.</returns>
    public static IEnumerable<NetworkInterface> GetInternetInterfaces() =>
        NetworkInterface.GetAllNetworkInterfaces()
                        .Where(n => n.OperationalStatus is OperationalStatus.Up
                                    && n.NetworkInterfaceType is not (NetworkInterfaceType.Tunnel or NetworkInterfaceType.Loopback)
                                    && n.NetworkInterfaceType is NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet
                                    && n.GetIPProperties().GatewayAddresses.Count != 0)
                        .OrderBy(n => n.NetworkInterfaceType is NetworkInterfaceType.Ethernet ? 1 : 0)
                        .ThenBy(n => n.Name);

    public static IPAddress? GetLanIp()
    {
        lock (lanIpLock)
        {
            if (lanIpCache != null)
            {
                return lanIpCache;
            }
        }

        IPAddress? ipv6Candidate = null;
        foreach (NetworkInterface ni in GetInternetInterfaces())
        {
            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                IPAddress address = ip.Address.TryExtractMappedIPv4();
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    lock (lanIpLock)
                    {
                        return lanIpCache = address;
                    }
                }

                if (ipv6Candidate == null && address is { AddressFamily: AddressFamily.InterNetworkV6, IsIPv6LinkLocal: false, IsIPv6Multicast: false })
                {
                    ipv6Candidate = address;
                }
            }
        }

        if (ipv6Candidate != null)
        {
            lock (lanIpLock)
            {
                return lanIpCache = ipv6Candidate;
            }
        }

        return null;
    }

    public static async Task<IPAddress?> GetWanIpAsync()
    {
        lock (wanIpLock)
        {
            if (wanIpCache != null)
            {
                return wanIpCache;
            }
        }

        IPAddress ip = await NatHelper.GetExternalIpAsync();
#if RELEASE
            if (ip == null || ip.IsPrivate())
            {
                Regex regex = new(@"(?:[0-2]??[0-9]{1,2}\.){3}[0-2]??[0-9]+");
                string[] sites =
                {
                    "https://ipv4.icanhazip.com/",
                    "https://checkip.amazonaws.com/",
                    "https://api.ipify.org/",
                    "https://api4.my-ip.io/ip",
                    "https://ifconfig.me/",
                    "https://showmyip.com/",
                };
                using HttpClient client = new();
                foreach (string site in sites)
                {
                    try
                    {
                        using HttpResponseMessage response = await client.GetAsync(site);
                        string content = await response.Content.ReadAsStringAsync();
                        ip = IPAddress.Parse(regex.Match(content).Value);
                        break;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
#endif

        lock (wanIpLock)
        {
            return wanIpCache = ip;
        }
    }

    public static IEnumerable<(IPAddress Address, string NetworkName)> GetVpnIps(params string[] vpnNetworkNames)
    {
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (!vpnNetworkNames.Contains(ni.Name, StringComparer.Ordinal))
            {
                continue;
            }

            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                IPAddress address = ip.Address.TryExtractMappedIPv4();
                if (address.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
                {
                    continue;
                }
                if (address is { AddressFamily: AddressFamily.InterNetworkV6 } and ({ IsIPv6LinkLocal: true } or { IsIPv6Multicast: true }))
                {
                    continue;
                }
                yield return (address, ni.Name.Replace("VPN", "").Trim());
            }
        }
    }

    /// <summary>
    ///     Gets supported VPN address if known by current machine.
    /// </summary>
    public static IEnumerable<(IPAddress Address, string NetworkName)> GetVpnIps() => GetVpnIps("Hamachi", "Radmin VPN");

    public static bool HasInternetConnectivity()
    {
        lock (connectivityLock)
        {
            if (hasInternet.HasValue && lastSeenPacketChange != -1 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastSeenPacketChange < TimeSpan.FromSeconds(5).TotalMilliseconds)
            {
                return hasInternet.Value;
            }
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (hasInternet = false).Value;
            }
            long prevNetBytes = lastBytesSendOrReceived;
            long currentNetBytes = GetTotalInternetBytes();
            currentNetBytes = currentNetBytes < 1 ? -1 : currentNetBytes;
            hasInternet = prevNetBytes != currentNetBytes;

            lastBytesSendOrReceived = currentNetBytes;
            lastSeenPacketChange = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return hasInternet.Value;
        }

        static long GetTotalInternetBytes() =>
            GetInternetInterfaces().Sum(i =>
            {
                IPInterfaceStatistics stats = i.GetIPStatistics();
                return stats.BytesReceived + stats.BytesSent;
            });
    }
}

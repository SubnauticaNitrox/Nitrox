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

namespace NitroxModel.Helper
{
    public static class NetHelper
    {
        private static readonly string[] privateNetworks =
            {
                "10.0.0.0/8",
                "127.0.0.0/8",
                "172.16.0.0/12",
                "192.0.0.0/24 ",
                "192.168.0.0/16",
                "198.18.0.0/15",
            };

        private static IPAddress wanIpCache;
        private static IPAddress lanIpCache;
        private static readonly object wanIpLock = new();
        private static readonly object lanIpLock = new();

        /// <summary>
        ///     Gets the network interfaces used for going onto the internet.
        ///     This is done by filtering for "Ethernet" and "Wi-Fi" network interfaces where "Ethernet" is returned earlier.
        /// </summary>
        /// <returns>Network interfaces used to go onto the internet.</returns>
        public static IEnumerable<NetworkInterface> GetInternetInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(n => n.OperationalStatus is OperationalStatus.Up
                                            && n.NetworkInterfaceType is not (NetworkInterfaceType.Tunnel or NetworkInterfaceType.Loopback)
                                            && n.NetworkInterfaceType is (NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet))
                                   .OrderBy(n => n.NetworkInterfaceType is NetworkInterfaceType.Ethernet ? 1 : 0)
                                   .ThenBy(n => n.Name);
        }

        public static IPAddress GetLanIp()
        {
            lock (lanIpLock)
            {
                if (lanIpCache != null)
                {
                    return lanIpCache;
                }
            }

            foreach (NetworkInterface ni in GetInternetInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        lock (lanIpLock)
                        {
                            return lanIpCache = ip.Address;
                        }
                    }
                }
            }
            
            return null;
        }

        public static async Task<IPAddress> GetWanIpAsync()
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

        public static IPAddress GetHamachiIp()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.Name != "Hamachi")
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address;
                    }
                }
            }
            return null;
        }

        public static async Task<bool> HasInternetConnectivityAsync()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }
            using Ping ping = new();
            PingReply reply = await ping.SendPingAsync(new IPAddress([8, 8, 8, 8]),2000);
            return reply.Status == IPStatus.Success;
        }

        /// <summary>
        ///     Returns true if the given IP address is reserved for private networks.
        /// </summary>
        public static bool IsPrivate(this IPAddress address)
        {
            static bool IsInRange(IPAddress ipAddress, string mask)
            {
                string[] parts = mask.Split('/');

                int ipNum = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
                int cidrAddress = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
                int cidrMask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

                return (ipNum & cidrMask) == (cidrAddress & cidrMask);
            }

            foreach (string privateSubnet in privateNetworks)
            {
                if (IsInRange(address, privateSubnet))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Returns true if the IP address points to the executing machine.
        /// </summary>
        public static bool IsLocalhost(this IPAddress address)
        {
            if (address == null)
            {
                return false;
            }
            if (IPAddress.IsLoopback(address))
            {
                return true;
            }

            foreach (NetworkInterface ni in GetInternetInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (address.Equals(ip.Address))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

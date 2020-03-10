using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NitroxModel.Logger;

namespace NitroxServer
{
    public static class IpLogger
    {
        public static void PrintServerIps()
        {
            try
            {
                NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface eachInterface in allInterfaces)
                {
                    PrintIfHamachi(eachInterface);
                    PrintIfLan(eachInterface);
                }

                PrintIfExternal();
            }
            catch (Exception)
            {
                // This is technically an error but will scare most users into thinking the server is not working
                // generally this can happen on Mac / Wine due to issues fetching networking interfaces.  Simply
                // ignore as this is not a big deal.  They can look these up themselves.
                Log.Info("Unable to resolve IP Addresses... you are on your own.");
            }
        }

        private static void PrintIfHamachi(NetworkInterface netInterface)
        {
            if (netInterface.Name != "Hamachi")
            {
                return;
            }

            IEnumerable<string> ips = netInterface.GetIPProperties().UnicastAddresses
                .Select(address => address.Address.ToString())
                .Where(address => !address.ToString().Contains("fe80::"));
            Log.Info("If using Hamachi, use this IP: " + string.Join(" or ", ips));
        }

        private static void PrintIfLan(NetworkInterface netInterface)
        {
            if (netInterface.GetIPProperties().GatewayAddresses.Count == 0)
            {
                return;
            }

            foreach (UnicastIPAddressInformation eachIp in netInterface.GetIPProperties().UnicastAddresses)
            {
                string[] splitIpParts = eachIp.Address.ToString().Split('.');
                int secondPart = 0;
                if (splitIpParts.Length > 1)
                {
                    int.TryParse(splitIpParts[1], out secondPart);
                }

                if (splitIpParts[0] == "10" || splitIpParts[0] == "192" && splitIpParts[1] == "168" || splitIpParts[0] == "172" && secondPart > 15 && secondPart < 32) //To get if IP is private
                {
                    Log.Info("If playing on LAN, use this IP: " + eachIp.Address);
                }
            }
        }

        private static void PrintIfExternal()
        {
            using (Ping ping = new Ping())
            {
                ping.PingCompleted += PingOnPingCompleted;
                ping.SendAsync("8.8.8.8", 1000, null);
            }
        }

        private static void PingOnPingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.Reply == null || e.Reply.Status != IPStatus.Success)
            {
                return;
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadStringCompleted += ExternalIpStringDownloadCompleted;
                client.DownloadStringAsync(new Uri("https://ipv4bot.whatismyipaddress.com/")); // from https://stackoverflow.com/questions/3253701/get-public-external-ip-address answer by user_v
            }
        }

        private static void ExternalIpStringDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                Log.Info($"If using port forwarding, use this IP: {e.Result}");
            }
            else
            {
                Log.Warn("Could not get your external IP. You are on your own...");
            }            
        }
    }
}

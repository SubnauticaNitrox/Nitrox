﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Linq;
using System.Net.NetworkInformation;
using NitroxModel.Logger;

namespace NitroxServer
{
    public static class IpLogger
    {
        public static void PrintServerIps()
        {
            NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach(NetworkInterface eachInterface in allInterfaces)
            {
                PrintIfHamachi(eachInterface);
                PrintIfLan(eachInterface);
            }
            PrintIfExternal();
        }
        private static void PrintIfHamachi(NetworkInterface _interface)
        {
            if (_interface.Name == "Hamachi")
            {
                var ips = _interface.GetIPProperties().UnicastAddresses
                .Select(address => address.Address.ToString())
                .Where(address => !address.ToString().Contains("fe80::"));
                Log.Info("If using Hamachi, use this IP: " + string.Join(" or ", ips));
            }
        }
        private static void PrintIfLan(NetworkInterface _interface)
        {
            if (!(_interface.GetIPProperties().GatewayAddresses.Count == 0)) // To avoid VMWare / other virtual interfaces
            {
                foreach (IPAddressInformation eachIP in _interface.GetIPProperties().UnicastAddresses)
                {
                    string[] splitIpParts = eachIP.Address.ToString().Split('.');
                    int secondPart = 0;
                    if (splitIpParts.Length > 1)
                    {
                        int.TryParse(splitIpParts[1], out secondPart);
                    }
                    if (splitIpParts[0] == "10" || (splitIpParts[0] == "192" && splitIpParts[1] == "168") || (splitIpParts[0] == "172" && (secondPart > 15 && secondPart < 32))) //To get if IP is private
                    {
                        Log.Info("If playing on LAN, use this IP: " + eachIP.Address.ToString());
                    }
                }
            }
        }
        private static void PrintIfExternal()
        {
            using (Ping checkConnectivity = new Ping())
            {
                if (checkConnectivity.Send("8.8.8.8", 1000).Status == IPStatus.Success) //Test internet connectivity before getting public IP
                {
                    using (WebClient client = new WebClient())
                    {
                        string externalIP = client.DownloadString("http://bot.whatismyipaddress.com"); // from https://stackoverflow.com/questions/3253701/get-public-external-ip-address answer by user_v
                        Log.Info("If using port forwarding, use this IP: " + externalIP);
                    }
                }
            }
        }
    }
}

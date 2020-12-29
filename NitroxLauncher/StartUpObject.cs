using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LibZeroTier;

namespace NitroxLauncher
{
    class StartUpObject
    {
        /// <summary>
        /// Application Entry Point.
        /// </summary>

        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                if (args[0].Equals("zerotiermiddleman"))
                {
                    ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
                    switch (args[1])
                    {
                        case "join":
                            JoinNet(args[2]);
                            break;
                        case "get":
                            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
                            File.WriteAllText(args[3], (nets.Count == 1 && (nets[0] ?? new ZeroTierNetwork() { NetworkStatus = "REQUESTING_CONFIGURATION" }).NetworkStatus == "OK" && (nets[0] ?? new ZeroTierNetwork() { IsConnected = false }).IsConnected == true).ToString());
                            break;
                    }
                    Environment.Exit(0);
                }
            }
            else
            {
                App.Main();
            }
        }
        private static void JoinNet(string serverId)
        {
            // get zero teir process(es)
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            if (Zero.Length <= 0)
            {
                // start zero teir
                Process proccess = new Process();
                proccess.StartInfo.FileName = @"C:\Program Files (x86)\ZeroTier\One\ZeroTier One.exe";
                proccess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proccess.StartInfo.CreateNoWindow = true;
                proccess.Start();
            }
            // initialize the connection handler
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
            // initialize the API Handler (normally done by joining a network, but we need to get the net list before we join)
            PrivateNetwork.ZeroTierHandler = new APIHandler();
            // get network list
            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            foreach (ZeroTierNetwork net in nets)
            {
                // leave any networks other than the desired network
                if (net.NetworkId != serverId)
                    PrivateNetwork.ZeroTierHandler.LeaveNetwork(net.NetworkId);
            }
            // update net list
            List<ZeroTierNetwork> Newnets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            bool update = true;
            // check if the network is/was already connected
            if (Newnets.Count == 1)
            {
                if (Newnets[0].NetworkId == serverId)
                {
                    update = false;
                    // wait for the network to fully connect
                    while (!PrivateNetwork.GetNetStatus().Equals("OK"))
                    { }
                }
            }
            // connect to network if it is not already connected to
            if (update)
            {
                // wait for the network to fully connect
                PrivateNetwork.JoinServerAsync(serverId).Wait();
            }
        }
    }
}

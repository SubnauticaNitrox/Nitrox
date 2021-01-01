using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LibZeroTier;

namespace NitroxLauncher
{
    class StartUpObject
    {
        public static string TargetId;
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
                    switch (args[1])
                    {
                        case "join":
                            JoinNet(args[2]);
                            break;
                        case "status":
                            TargetId = args[2];
                            ExitCodeStatus();
                            break;
                        case "leaveall":
                            LeaveAllNetworks();
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
        private static void LeaveAllNetworks()
        {
            // initialize the connection handler
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI(null, null);
            PrivateNetwork.StopServerAsync(false);
            Environment.Exit(0);
        }
        private static void JoinNet(string serverId)
        {
            // initialize the connection handler
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI(new API_Settings() { }, new Network_Settings() { }, true);
            // get network list
            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            foreach (ZeroTierNetwork net in nets)
            {
                // leave any networks other than the desired network
                if (net.NetworkId != serverId)
                    PrivateNetwork.ZeroTierHandler.LeaveNetwork(net.NetworkId);
            }
            // update net list
            nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            bool update = true;
            // check if the network is/was already connected
            if (nets.Count == 1)
            {
                if (nets[0].NetworkId == serverId)
                {
                    update = false;
                    // wait for the network to fully connect
                    while (!PrivateNetwork.GetPrimaryNetworkStatus().Equals("OK")){ }
                }
            }
            // connect to network if it is not already connected to
            if (update)
            {
                // retsart zerotier
                PrivateNetwork.RestartZeroTier();
                // wait for the network to fully connect
                PrivateNetwork.JoinServerAsync(serverId).Wait();
            }
        }
        private static void ExitCodeStatus()
        {
            // get zero teir process(es)
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            if (Zero.Length <= 0)
            {
                // exits with exit code "ZeroTier One not running"
                Environment.Exit(18);
            }
            // initialize the connection handler
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI(null, null);
            PrivateNetwork.NetworkChangeEvent += PrivateNetwork_NetworkChangeEvent;
            // initial check
            CheckNetStatus(PrivateNetwork, 124);
            // loop forever until app exit
            while (true)
            { 
                // get subnautica process(es)
                Zero = Process.GetProcessesByName("Subnautica");
                if (Zero.Length <= 0)
                {
                    // exits normally
                    Environment.Exit(0);
                }
                Task.Delay(250).Wait();
            }
        }

        private static void PrivateNetwork_NetworkChangeEvent(object sender, ZeroTierAPI.NetworkChangedEventArgs e)
        {
            StatusChange[] AcceptableChangesList = new StatusChange[] 
            { 
                StatusChange.AllowDefault, 
                StatusChange.AllowDNS,
                StatusChange.AllowManaged,
                StatusChange.AllowGlobal,
                StatusChange.AssignedAddresses,
                StatusChange.GenericPropertyChange, 
                StatusChange.BroadcastEnabled, 
                StatusChange.DeviceName,
                StatusChange.MTU,
                StatusChange.Dns,
                StatusChange.MulticastSubscriptions,
                StatusChange.NetconfRevision,
                StatusChange.NetworkType,
                StatusChange.NetworkName,
                StatusChange.Routes,
                StatusChange.DHCP
            };
            if(Array.IndexOf(AcceptableChangesList, e.Change) < 0)
            {
                switch (e.Change)
                {
                    case StatusChange.NetworkList:
                        NetListChanged(sender, 124);
                        break;
                    case StatusChange.ConnectionTimeout:
                        Environment.Exit(408);
                        break;
                    case StatusChange.IsConnected:
                        CheckNetStatus(sender as ZeroTierAPI, 117);
                        break;
                    case StatusChange.LeftNetwork:
                        CheckNetStatus(sender as ZeroTierAPI, 119);
                        break;
                    case StatusChange.DestroyedNetwork:
                        CheckNetStatus(sender as ZeroTierAPI, 119);
                        break;
                    case StatusChange.JoinedNetwork:
                        CheckNetStatus(sender as ZeroTierAPI, 122);
                        break;
                    case StatusChange.CreatedNetwork:
                        CheckNetStatus(sender as ZeroTierAPI, 122);
                        break;
                    case StatusChange.NetworkId:
                        CheckNetStatus(sender as ZeroTierAPI, 126);
                        break;
                    case StatusChange.NetworkStatus:
                        CheckNetStatus(sender as ZeroTierAPI, 131);
                        break;
                    case StatusChange.PortError:
                        Environment.Exit(916);
                        break;
                    case StatusChange.UnexpectedShutdown:
                        Environment.Exit(18);
                        break;
                    case StatusChange.UserStatus:
                        Environment.Exit(29);
                        break;

                }
            }
        }
        public static void CheckNetStatus(ZeroTierAPI sender, int code)
        {
            List<ZeroTierNetwork> nets = sender.ZeroTierHandler.GetNetworks();
            if (nets.Count != 1 || nets[0]?.NetworkStatus.Equals("OK") != true || nets[0]?.NetworkId != TargetId)
                Environment.Exit(code);
        }
        public static void NetListChanged(object sender, int code)
        {
            if(((ZeroTierAPI)sender).ZeroTierHandler.GetNetworks().Count == 0)
                Environment.Exit(119);
            if (((ZeroTierAPI)sender).ZeroTierHandler.GetNetworks().Count > 1)
                Environment.Exit(122);
            if (((ZeroTierAPI)sender).ZeroTierHandler.GetNetworks()[0]?.NetworkId != TargetId)
                Environment.Exit(code);
        }
    }
}

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
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
            // initialize the API Handler (normally done by joining a network, but we need to get the net list before we join)
            PrivateNetwork.ZeroTierHandler = new APIHandler();
            // get network list
            List<ZeroTierNetwork> nets = PrivateNetwork.ZeroTierHandler.GetNetworks();
            foreach (ZeroTierNetwork net in nets)
            {
                // leave all networks
                PrivateNetwork.ZeroTierHandler.LeaveNetwork(net.NetworkId);
            }
            PrivateNetwork.DeleteAllNonConnectedNetworks();
            // get zero teir process(es) and kill em
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            foreach (Process item in Zero) { item.Kill(); }
            foreach (Process item in Zero) { item.WaitForExit(); }
            Environment.Exit(0);
        }
        private static void JoinNet(string serverId)
        {
            // get zero teir process(es)
            Process[] Zero = Process.GetProcessesByName("ZeroTier One");
            // start zerotier if its not already started
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
            ZeroTierAPI PrivateNetwork = new ZeroTierAPI();
            PrivateNetwork.ZeroTierHandler = new APIHandler();
            PrivateNetwork.ZeroTierHandler.AddEventHandler(PrivateNetwork.ZeroTierHandler_NetworkChangeEvent);
            PrivateNetwork.NetworkChangeEvent += PrivateNetwork_NetworkChangeEvent;
            // initial check
            CheckNetStatus(PrivateNetwork, 124);
            // loop forever until app exit
            while (true)
            { 
                // get zero teir process(es)
                Zero = Process.GetProcessesByName("ZeroTier One");
                if (Zero.Length <= 0)
                {
                    // exits with exit code "ZeroTier One not running"
                    Environment.Exit(18);
                }
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

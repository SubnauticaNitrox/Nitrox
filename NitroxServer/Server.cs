using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Communication.Packets;
using NitroxServer.Serialization.World;
using System.Timers;
using NitroxServer.ConfigParser;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;


namespace NitroxServer
{
    public class Server
    {
        private readonly World world;
        private readonly UdpServer udpServer;
        private readonly WorldPersistence worldPersistence;
        private readonly PacketHandler packetHandler;
        private readonly Timer saveTimer;
        private ServerConfigReader ServerOptions;
        public static Server Instance;

        public void Save()
        {
            worldPersistence.Save(world);
        }

        public Server(ServerConfigReader configReader)
        {
            ServerOptions = configReader;
            Instance = this;
            worldPersistence = new WorldPersistence();
            world = worldPersistence.Load();
            packetHandler = new PacketHandler(world);
            udpServer = new UdpServer(packetHandler, world.PlayerManager, world.EntitySimulation, ServerOptions);

            //Maybe add settings for the interval?
            saveTimer = new Timer();
            saveTimer.Interval = 60000;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate
            {
                Save();
            };
        }

        public void Start()
        {
            ListServerIPs();
            udpServer.Start();
            Log.Info("Nitrox Server Started");
            EnablePeriodicSaving();
        }
        public void ListServerIPs()
        {
            NetworkInterface[] allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface eachInterface in allInterfaces)
            {
                if (eachInterface.Name == "Hamachi")
                {
                    var ips = eachInterface.GetIPProperties().UnicastAddresses
                    .Select(address => address.Address.ToString())
                    .Where(address => !address.ToString().Contains("fe80::"));
                    Log.Info("If using Hamachi, use this IP: " + string.Join(" or ", ips));
                }
                if (!(eachInterface.GetIPProperties().GatewayAddresses.Count == 0)) // To avoid VMWare / other virtual interfaces
                {
                    foreach (IPAddressInformation eachIP in eachInterface.GetIPProperties().UnicastAddresses)
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
            using(Ping checkConnectivity = new Ping())
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
        public void Stop()
        {
            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicSaving();
            Save();
            udpServer.Stop();
            Log.Info("Nitrox Server Stopped");
        }
        
        private void EnablePeriodicSaving()
        {
            saveTimer.Start();
        }

        private void DisablePeriodicSaving()
        {
            saveTimer.Stop();
        }
    }
}

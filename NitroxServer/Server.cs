using System.Timers;
using NitroxModel.Logger;
using NitroxServer.Communication;
using NitroxServer.Serialization.World;
using NitroxServer.ConfigParser;

namespace NitroxServer
{
    public class Server
    {
        private readonly Timer saveTimer;
        private readonly UdpServer udpServer;
        private readonly World world;
        private readonly WorldPersistence worldPersistence;
        public bool IsRunning { get; private set; }
        public static Server Instance { get; private set; }

        private readonly string serverInfoApi = "http://nitrox.qaq.link/notify";
        private readonly ServerConfig serverConfig;
        private readonly Timer pingTimer;
        private string externalIP = null;
        private string pingerGUID = null;
        private int playerNum;


        public Server(WorldPersistence worldPersistence, World world, UdpServer udpServer, ServerConfig serverConfig)
        {
            Instance = this;
            this.worldPersistence = worldPersistence;
            this.world = world;
            this.udpServer = udpServer;
            this.serverConfig = serverConfig;

            saveTimer = new Timer();
            saveTimer.Interval = serverConfig.SaveInterval;
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += delegate { Save(); };
            
            pingTimer = new Timer();
            pingTimer.Interval = 10*60*1000;
            pingTimer.AutoReset = true;
            pingTimer.Elapsed += delegate { Ping(true); };

        }

        public void Save()
        {
            worldPersistence.Save(world);
        }

        /// <summary>
        /// Notify server manager this server online
        /// </summary>
        public void Ping(bool online)
        {
            if (!serverConfig.BroadcastServer)
            {
                return;
            }

            // externalIP is set to "" if no external IP address is avaliable
            if (externalIP != null && externalIP == "")
            {
                return;
            }
            if (pingerGUID == null)
            {
                pingerGUID = System.Guid.NewGuid().ToString("N");
            }
            
            new System.Threading.Thread(()=>
            {
                string op = online ? "online" : "offline";
                if (online) playerNum = udpServer.GetPlayerCount();

                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    if (externalIP == null)
                    {
                        // ipv4 first.
                        try
                        {
                            externalIP = client.DownloadString(new System.Uri("http://ipv4bot.whatismyipaddress.com/"));
                        }
                        catch
                        {
                            try
                            {
                                externalIP = client.DownloadString(new System.Uri("http://ipv6bot.whatismyipaddress.com/"));
                            }
                            catch
                            {
                                externalIP = "";
                                Log.Error("Faild to retrieve external IP address");
                            }
                        }
                    }
                    if (externalIP == null || externalIP == "") return;

                    string broadcastServerName = serverConfig.BroadcastServerName == "" ? externalIP : serverConfig.BroadcastServerName;
                    string escaped_serverinfo = System.Uri.EscapeDataString($"{broadcastServerName}|{externalIP}:{serverConfig.ServerPort}|{playerNum}");
                    string notifyurl = $"{serverInfoApi}/{op}/{pingerGUID}/{escaped_serverinfo}";
                    try
                    {
                        string ret = client.DownloadString(new System.Uri(notifyurl));
                        Log.Info("Broadcast Server: " + ret);
                    }
                    catch { Log.Error("Broadcast server error"); }
                }
            }).Start();
        }

        public void Start()
        {
            IsRunning = true;
            IpLogger.PrintServerIps();
            udpServer.Start();
            Log.Info("Nitrox Server Started");
            EnablePeriodicSaving();
            EnablePeriodicPinging();
        }

        public void Stop()
        {
            Log.Info("Nitrox Server Stopping...");
            DisablePeriodicPinging();
            DisablePeriodicSaving();
            Save();
            udpServer.Stop();
            Log.Info("Nitrox Server Stopped");
            IsRunning = false;
        }

        private void EnablePeriodicSaving()
        {
            saveTimer.Start();
        }

        private void DisablePeriodicSaving()
        {
            saveTimer.Stop();
        }

        private void EnablePeriodicPinging()
        {
            Ping(true);
            pingTimer.Start();
        }

        private void DisablePeriodicPinging()
        {
            pingTimer.Stop();
            Ping(false);
        }

    }
}

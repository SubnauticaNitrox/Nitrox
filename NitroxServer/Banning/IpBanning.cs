using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxServer.Banning
{
    public static class IpBanning
    {
        public static List<string> BannedIPs = new List<string>();
        public static string BanFilePath { get; } = Path.GetFullPath(Path.Combine(Environment.GetEnvironmentVariable("NITROX_LAUNCHER_PATH") ?? "", "Player Bans.txt"));

        public static void Load()
        {
            if (File.Exists(BanFilePath))
            {
                var fileStrings = File.ReadAllLines(BanFilePath);
                BannedIPs.AddRange(fileStrings);

            }
        }

        public static void Save()
        {
            File.WriteAllLines(BanFilePath, BannedIPs);
        }

        public static void AddNewBan(System.Net.IPAddress address)
        {
            if (!BannedIPs.Contains(address.ToString())){
                BannedIPs.Add(address.ToString());
                Save();
            }
        }

        public static void AddNewBan(string address)
        {
            if (!BannedIPs.Contains(address))
            {
                BannedIPs.Add(address);
                Save();
            }
        }

        public static void RemoveBan(System.Net.IPAddress address)
        {
            if (BannedIPs.Contains(address.ToString()))
            {
                BannedIPs.Remove(address.ToString());
                Save();
            }
        }

        public static void RemoveBan(string address)
        {
            if (BannedIPs.Contains(address))
            {
                BannedIPs.Remove(address);
                Save();
            }
        }
    }
}

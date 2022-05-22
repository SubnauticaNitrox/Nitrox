using System.Net.NetworkInformation;
using NitroxLauncher.Models.Troubleshoot.Abstract;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class NetworkModule : TroubleshootModule
    {
        public NetworkModule()
        {
            Name = "Network";
        }

        protected override bool Check()
        {
            // Check if there is any network connection available
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                EmitLog("Unable to find any available network connection");
                return false;
            }

            EmitLog("Found an available network connection");

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface iface in interfaces)
            {
                if (iface.Name.Contains("Hamachi"))
                {
                    EmitLog($"Found hamachi adapter : STATUS {iface.OperationalStatus}");
                }

                if (iface.Name.Contains("Radmin"))
                {
                    EmitLog($"Found Radmin adapter : STATUS {iface.OperationalStatus}");
                }

                if (iface.OperationalStatus == OperationalStatus.Up && iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties properties = iface.GetIPProperties();
                    IPv4InterfaceStatistics stats = iface.GetIPv4Statistics();

                    EmitLog($"Found an active interface :\n - Name: {iface.Name} \n - Description: {iface.Description}\n - NetworkInterfaceType: {iface.NetworkInterfaceType}\n - Speed (kilobits per second): {iface.Speed / 1000}\n - Speed (megabits per second): {iface.Speed / 1000 / 1000}\n - IsReceiveOnly: {iface.IsReceiveOnly}\n - SupportsMulticast: {iface.SupportsMulticast}]\n");
                }
            }

            return true;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using ToastNotifications.Messages.Error;
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper.FirewallRules;

namespace NitroxLauncher.Models.Utils
{
    internal static class WindowsHelper
    {
        public static string ProgramFileDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

        internal static bool IsAppRunningInAdmin()
        {
            WindowsPrincipal wp = new(WindowsIdentity.GetCurrent());
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static void RestartAsAdmin()
        {
            if (!IsAppRunningInAdmin())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Nitrox launcher should be executed with administrator permissions in order to properly patch Subnautica while in Program Files directory, do you want to restart ?",
                    "Nitrox needs permissions",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.Yes,
                    MessageBoxOptions.DefaultDesktopOnly
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Setting up start info of the new process of the same application
                        ProcessStartInfo processStartInfo = new(Assembly.GetEntryAssembly().CodeBase);

                        // Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
                        processStartInfo.UseShellExecute = true;
                        processStartInfo.Verb = "runas";

                        // Start the application as new process
                        Process.Start(processStartInfo);
                        Environment.Exit(1);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error while trying to instance an admin processus of the launcher, aborting");
                    }
                }
            }
            else
            {
                Log.Info("Can't restart the launcher as administrator, we already have permissions");
            }
        }

        internal static void CheckFirewallRules()
        {
            try
            {
                CheckFirewallRules(FirewallDirection.Inbound);
                CheckFirewallRules(FirewallDirection.Outbound);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("There was a problem configuring the Windows firewall. Try restarting the launcher as administrator or manually adding firewall rules for Nitrox programs.");
            }
        }

        internal static void CheckFirewallRules(FirewallDirection direction)
        {
            CheckClientFirewallRules(direction);
            CheckServerFirewallRules(direction);
            CheckHamachiFirewallRules(direction);
        }

        private static void CheckServerFirewallRules(FirewallDirection direction)
        {
            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ServerLogic.SERVER_EXECUTABLE);

            AddExclusiveFirewallRule(serverPath, serverPath, direction);
        }

        private static void CheckClientFirewallRules(FirewallDirection direction)
        {
            string clientPath = Path.Combine(LauncherLogic.Config.SubnauticaPath, "Subnautica.exe");

            AddExclusiveFirewallRule(clientPath, clientPath, direction);
        }

        private static void CheckHamachiFirewallRules(FirewallDirection direction)
        {
            static IPAddress GetHamachiAddress()
            {
                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.Name == "Hamachi")
                    {
                        foreach (UnicastIPAddressInformation addressInfo in networkInterface.GetIPProperties().UnicastAddresses)
                        {
                            if (addressInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                return addressInfo.Address;
                            }
                        }
                    }
                }

                return null;
            }

            const string NAME = "HamachiNitrox";

            if (GetHamachiAddress() == null || !FirewallWASRule.IsLocallySupported)
            {
                return;
            }

            foreach (IFirewallRule firewallRule in FirewallManager.Instance.Rules)
            {
                if (firewallRule.Name == NAME)
                {
                    return;
                }
            }

            FirewallWASRule rule = new(NAME, FirewallAction.Allow, direction, FirewallManager.Instance.GetActiveProfile().Type)
            {
                Description = "Ignore Hamachi network",
                Protocol = FirewallProtocol.Any,
                RemoteAddresses = new[] { new NetworkAddress(GetHamachiAddress()) },
                IsEnable = true
            };

            FirewallManager.Instance.Rules.Add(rule);
        }

        private static void AddExclusiveFirewallRule(string name, string filePath, FirewallDirection direction)
        {
            if (!FirewallRuleExists(name, direction))
            {
                AddFirewallRule(name, filePath, direction);
            }
        }

        private static bool FirewallRuleExists(string name, FirewallDirection direction) => FirewallManager.Instance.Rules.Any(
            rule => rule.FriendlyName == name && rule.Direction == direction);

        private static void AddFirewallRule(string name, string filePath, FirewallDirection direction)
        {
            IFirewallRule rule = FirewallManager.Instance.CreateApplicationRule(name, FirewallAction.Allow, filePath);
            rule.Direction = direction;
            rule.Protocol = FirewallProtocol.Any;
            rule.IsEnable = true;

            FirewallManager.Instance.Rules.Add(rule);
        }
    }
}

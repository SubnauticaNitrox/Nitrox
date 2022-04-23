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
using WindowsFirewallHelper;
using WindowsFirewallHelper.Addresses;
using WindowsFirewallHelper.FirewallRules;
using NitroxModel;

namespace NitroxLauncher.Models.Utils
{
    internal static class WindowsHelper
    {
        public static string ProgramFileDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%");

        private const string HAMACHI_FIREWALL_RULE_NAME = "HamachiNitrox";

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
            catch (FileNotFoundException ex)
            {
                Log.Warn($"Tried to add firewall rule for program that does not exist: {ex.FileName}");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Try restarting the launcher as administrator or manually adding firewall rules for Nitrox programs. This warning won't be shown again.", "Error adding Windows Firewall rules", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static void CheckFirewallRules(FirewallDirection direction)
        {
            CheckClientFirewallRules(direction);
            CheckServerFirewallRules(direction);
            CheckHamachiFirewallRules(direction);
        }

        private static void CheckServerFirewallRules(FirewallDirection direction)
        {
            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ServerLogic.SERVER_EXECUTABLE);

            AddExclusiveFirewallRule(Path.GetFileName(serverPath), serverPath, direction);
        }

        private static void CheckClientFirewallRules(FirewallDirection direction)
        {
            string clientPath = Path.Combine(LauncherLogic.Config.SubnauticaPath, GameInfo.Subnautica.ExeName);

            AddExclusiveFirewallRule(Path.GetFileName(clientPath), clientPath, direction);
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

            if (GetHamachiAddress() == null || !FirewallWASRule.IsLocallySupported)
            {
                return;
            }

            foreach (IFirewallRule firewallRule in FirewallManager.Instance.Rules)
            {
                if (firewallRule.Name == HAMACHI_FIREWALL_RULE_NAME)
                {
                    return;
                }
            }

            FirewallWASRule rule = new(HAMACHI_FIREWALL_RULE_NAME, FirewallAction.Allow, direction, FirewallManager.Instance.GetActiveProfile().Type)
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
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Unable to add firewall rule to non-existent program", filePath);
            }
            if (!FirewallRuleExists(name, filePath, direction))
            {
                AddFirewallRule(name, filePath, direction);
            }
        }

        private static bool FirewallRuleExists(string name, string programPath, FirewallDirection direction) => FirewallManager.Instance.Rules.Any(rule => rule.FriendlyName == name && rule.Direction == direction && (programPath?.Equals(rule.ApplicationName, StringComparison.InvariantCultureIgnoreCase) ?? true));

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

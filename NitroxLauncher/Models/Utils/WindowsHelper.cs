using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using WindowsFirewallHelper;

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

        internal static void CheckServerFirewallRules()
        {
            string serverRuleName = "nitroxserver-subnautica.exe";
            string serverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ServerLogic.SERVER_EXECUTABLE);

            if (!FirewallRuleExists(serverRuleName))
            {
                AddFirewallRule(serverRuleName, serverPath);
            }
        }

        private static bool FirewallRuleExists(string name) => FirewallManager.Instance.Rules.Any(rule => rule.FriendlyName == name);

        private static void AddFirewallRule(string name, string filePath)
        {
            IFirewallRule rule = FirewallManager.Instance.CreateApplicationRule(name, FirewallAction.Allow, filePath);
            rule.Direction = FirewallDirection.Inbound;
            rule.Protocol = FirewallProtocol.Any;
            rule.IsEnable = true;

            FirewallManager.Instance.Rules.Add(rule);
        }
    }
}

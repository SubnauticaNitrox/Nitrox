using System;
using System.IO;
using System.Reflection;
using System.Threading;
using NitroxLauncher.Models.Troubleshoot.Abstract;
using NitroxLauncher.Models.Utils;
using NitroxModel;
using WindowsFirewallHelper;

namespace NitroxLauncher.Models.Troubleshoot
{
    internal class FirewallModule : TroubleshootModule
    {
        public static readonly string[] Paths = new string[]
        {
            Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), ServerLogic.SERVER_EXECUTABLE), // Nitrox server executable
            Path.Combine(LauncherLogic.Config.SubnauticaPath, GameInfo.Subnautica.ExeName), // Subnautica game executable
        };

        public FirewallModule()
        {
            Name = "Firewall";
        }

        protected override bool Check()
        {
            Thread.Sleep(5000);
            int steps = 0;

            try
            {
                if (!CheckFirewallRules(FirewallDirection.Inbound))
                {
                    steps++;
                }

                if (!CheckFirewallRules(FirewallDirection.Outbound))
                {
                    steps++;
                }
            }
            catch (FileNotFoundException ex)
            {
                EmitLog($"Tried to add firewall rule for program that does not exist: {ex.FileName}");
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                EmitLog("Unable to access firewall rules, try to restart the launcher with administrator rights");
                throw;
            }

            return steps == 0;
        }

        internal bool CheckFirewallRules(FirewallDirection direction)
        {
            int steps = Paths.Length;

            foreach (string path in Paths)
            {
                if (!CheckRuleByName(Path.GetFileName(path), path, direction))
                {
                    steps++;
                }
            }

            return steps == 0;
        }

        internal bool CheckRuleByName(string name, string programPath, FirewallDirection direction)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(programPath))
            {
                throw new ArgumentException("Invalid program name/path specified");
            }

            if (!File.Exists(programPath))
            {
                EmitLog($"Tried to add firewall rule for program that does not exist: {name}");
                return false;
            }

            if (!WindowsHelper.FirewallRuleExists(name, programPath, direction))
            {
                EmitLog($"Couldn't find an existing {direction} rule for `{name}` at `{programPath}`");
                return false;
            }

            EmitLog($"Found an existing {direction} rule for `{name}` at `{programPath}`");
            return true;
        }

    }
}

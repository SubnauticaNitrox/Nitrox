using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace InstallerActions.Patches
{
    public static class FirewallRules
    {
        public static string SetFirewallRules(string[] RulesArray, string SubPath)
        {
            Process setFirewallRules = new Process();
            ProcessStartInfo setFirewallRulesStartInfo = new ProcessStartInfo("netsh.exe");
            setFirewallRulesStartInfo.CreateNoWindow = true;
            setFirewallRulesStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            setFirewallRulesStartInfo.UseShellExecute = false;
            setFirewallRulesStartInfo.RedirectStandardOutput = true;
            Char quotation = '"';
            string status = "success";
            foreach (string item in RulesArray)
            {
                if (item == "PortsTCP" || item == "PortsUDP" || item == "ProgramServer" || item == "ProgramClient")
                {
                    if (item == "PortsTCP")
                    {
                        setFirewallRulesStartInfo.Arguments = "advfirewall firewall add rule name=" + quotation + "Nitrox Multiplayer Mod TCP" + quotation + " dir=in action=allow protocol=TCP localport=11000";
                    }
                    if (item == "PortsUDP")
                    {
                        setFirewallRulesStartInfo.Arguments = "advfirewall firewall add rule name=" + quotation + "Nitrox Multiplayer Mod UDP" + quotation + " dir=in action=allow protocol=UDP localport=11000";
                    }
                    if (item == "ProgramServer")
                    {
                        setFirewallRulesStartInfo.Arguments = "advfirewall firewall add rule name=" + quotation + "Nitrox Multiplayer Mod Server" + quotation + " dir=in action=allow program=" + quotation + SubPath + @"\SubServer\NitroxServer.exe" + quotation + " enable=yes";
                    }
                    if (item == "ProgramClient")
                    {
                        setFirewallRulesStartInfo.Arguments = "advfirewall firewall add rule name=" + quotation + "Nitrox Multiplayer Mod Client" + quotation + " dir=in action=allow program=" + quotation + SubPath + @"\Subnautica.exe" + quotation + " enable=yes";
                    }
                    setFirewallRules.StartInfo = setFirewallRulesStartInfo;
                    setFirewallRules.Start();
                    string output = setFirewallRules.StandardOutput.ReadToEnd();
                    setFirewallRules.WaitForExit();
                    setFirewallRules.Close();
                    if (!output.Contains("Ok."))
                    {
                        status = "fail";
                    }
                }
            }
            return status;
        }
        public static string RemoveFirewallRules(string[] RulesArray)
        {
            Process removeFirewallRules = new Process();
            ProcessStartInfo removeFirewallRulesStartInfo = new ProcessStartInfo("netsh.exe");
            removeFirewallRulesStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            removeFirewallRulesStartInfo.CreateNoWindow = true;
            removeFirewallRulesStartInfo.UseShellExecute = false;
            removeFirewallRulesStartInfo.RedirectStandardOutput = true;
            string status = "success";
            Char quotation = '"';
            foreach (string item in RulesArray)
            {
                if (item == "PortsTCP" || item == "PortsUDP" || item == "ProgramServer" || item == "ProgramClient")
                {
                    if (item == "PortsTCP")
                    {
                        removeFirewallRulesStartInfo.Arguments = "advfirewall firewall delete rule name=" + quotation + "Nitrox Multiplayer Mod TCP" + quotation + " protocol=TCP localport=11000";
                    }
                    if (item == "PortsUDP")
                    {
                        removeFirewallRulesStartInfo.Arguments = "advfirewall firewall delete rule name=" + quotation + "Nitrox Multiplayer Mod UDP" + quotation + " protocol=UDP localport=11000";
                    }
                    if (item == "ProgramServer")
                    {
                        removeFirewallRulesStartInfo.Arguments = "advfirewall firewall delete rule name=" + quotation + "Nitrox Multiplayer Mod Server" + quotation;
                    }
                    if (item == "ProgramClient")
                    {
                        removeFirewallRulesStartInfo.Arguments = "advfirewall firewall delete rule name=" + quotation + "Nitrox Multiplayer Mod Client" + quotation;
                    }
                    removeFirewallRules.StartInfo = removeFirewallRulesStartInfo;
                    removeFirewallRules.Start();
                    string output = removeFirewallRules.StandardOutput.ReadToEnd();
                    removeFirewallRules.WaitForExit();
                    removeFirewallRules.Close();
                    if (!output.Contains("Ok."))
                    {
                        status = "fail";
                    }
                }
            }
            return status;
        }
        public static string[] GetInstalledRules()
        {
            string[] installedRules = new string[4] { "none", "none", "none", "none" };
            Process getFirewallRules = new Process();
            ProcessStartInfo getFirewallRulesStartInfo = new ProcessStartInfo("netsh.exe");
            getFirewallRulesStartInfo.CreateNoWindow = true;
            getFirewallRulesStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            getFirewallRulesStartInfo.UseShellExecute = false;
            getFirewallRulesStartInfo.RedirectStandardOutput = true;
            char quotation = '"';
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod TCP" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output1 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (!output1.Contains("No rules match the specified criteria"))
            {
                installedRules[0] = "PortsTCP";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod UDP" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output2 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (!output2.Contains("No rules match the specified criteria"))
            {
                installedRules[1] = "PortsUDP";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod Server" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output3 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (!output3.Contains("No rules match the specified criteria"))
            {
                installedRules[2] = "ProgramServer";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod Client" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output4 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (!output4.Contains("No rules match the specified criteria"))
            {
                installedRules[3] = "ProgramClient";
            }
            return installedRules;
        }

        public static string[] GetRequiredRules()
        {
            string[] requiredNitroxRules = new string[4] { "none", "none", "none", "none" };
            Process getFirewallRules = new Process();
            ProcessStartInfo getFirewallRulesStartInfo = new ProcessStartInfo("netsh.exe");
            getFirewallRulesStartInfo.CreateNoWindow = true;
            getFirewallRulesStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            getFirewallRulesStartInfo.UseShellExecute = false;
            getFirewallRulesStartInfo.RedirectStandardOutput = true;
            char quotation = '"';
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod TCP" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output1 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (output1.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules[0] = "PortsTCP";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod UDP" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output2 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (output2.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules[1] = "PortsUDP";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod Server" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output3 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (output3.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules[2] = "ProgramServer";
            }
            getFirewallRulesStartInfo.Arguments = "advfirewall firewall show rule name=" + quotation + "Nitrox Multiplayer Mod Client" + quotation;
            getFirewallRules.StartInfo = getFirewallRulesStartInfo;
            getFirewallRules.Start();
            string output4 = getFirewallRules.StandardOutput.ReadToEnd();
            getFirewallRules.WaitForExit();
            getFirewallRules.Close();
            if (output4.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules[3] = "ProgramClient";
            }
            return requiredNitroxRules;
        }
    }
}


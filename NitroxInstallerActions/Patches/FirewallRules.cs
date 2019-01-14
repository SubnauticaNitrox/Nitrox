using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace InstallerActions.Patches
{
    public struct FirewallRule
    {
        string RunNetshCommand(string Argument)
        {
            string output = "";
            Process netshProcess = new Process();
            ProcessStartInfo netshStartInfo = new ProcessStartInfo("netsh.exe");
            netshStartInfo.CreateNoWindow = true;
            netshStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            netshStartInfo.UseShellExecute = false;
            netshStartInfo.RedirectStandardOutput = true;
            netshStartInfo.Arguments = Argument;
            netshProcess.StartInfo = netshStartInfo;
            netshProcess.Start();
            output = netshProcess.StandardOutput.ReadToEnd();
            netshProcess.WaitForExit();
            netshProcess.Close();
            return output;
        }
        public string addCommand;
        public string removeCommand;
        public string name;
        public bool isInstalled;
        string[] GetAddRemoveCommand(string RuleName, string GamePath, int Port = 11000)
        {
            string[] commands = new string[2] { "", "" };
            if (RuleName == "PortsUDP")
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod UDP\x22 dir=in action=allow protocol=UDP localport=" + Port;
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod UDP\x22 protocol=UDP localport=" + Port;
            }
            if (RuleName == "ProgramServer")
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Server\x22 dir=in action=allow program=\x22" + Path.Combine(GamePath, "SubServer", "NitroxServer.exe") + "\x22 enable=yes";
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Server\x22";
            }
            if (RuleName == "ProgramClient")
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Client\x22 dir=in action=allow program=\x22" + Path.Combine(GamePath, "Subnautica.exe") + "\x22 enable=yes";
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Client\x22";
            }
            return commands;
        }
        bool GetRuleInstalled(string RuleName)
        {
            string command = "", output = "";
            if (RuleName == "PortsUDP")
            {
                command = "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod UDP\x22";
            }
            if (RuleName == "ProgramServer")
            {
                command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Server\x22";
            }
            if (RuleName == "ProgramClient")
            {
                command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Client\x22";
            }
            output = RunNetshCommand(command);
            if(output.Contains("No rules match the specified criteria"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void CreateRule(string RuleName, string GamePath, int Port = 11000)
        {
            if (RuleName == "PortsUDP" || RuleName == "ProgramServer" || RuleName == "ProgramClient")
            {
                isInstalled = GetRuleInstalled(RuleName);
                string[] commands = GetAddRemoveCommand(RuleName, GamePath, Port);
                addCommand = commands[0];
                removeCommand = commands[1];
                name = RuleName;
            }
            else
            {
                throw new ArgumentException("Invalid rule type given. Rule type given was " + RuleName + ". It must be PortsUDP, ProgramServer, or ProgramClient");
            }
        }
        public bool Apply(bool IsInstalling)
        {
            if (IsInstalling)
            {
                if (isInstalled)
                {
                    return true;
                }
                string output = RunNetshCommand(addCommand);
                if (output.Contains("Ok."))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (!isInstalled)
                {
                    return true;
                }
                string output = RunNetshCommand(removeCommand);
                if (output.Contains("Ok."))
                {
                    return true;
                }
                return false;
            }
        }
    }
    public class FirewallRules
    {
        string subPath;
        int portNumber;
        List<FirewallRule> allRules;
        public FirewallRules(string GamePath, int PortNumber = 11000)
        {
            subPath = GamePath;
            portNumber = PortNumber;
            allRules = Init();
        }
        List<FirewallRule> Init()
        {
            List<FirewallRule> rules = new List<FirewallRule>();
            string[] ruleNames = new string[3] { "PortsUDP", "ProgramServer", "ProgramClient" };
            foreach(string eachName in ruleNames)
            {
                FirewallRule rule = new FirewallRule();
                rule.CreateRule(eachName, subPath, portNumber);
                rules.Add(rule);
            }
            return rules;
        }
        public bool InstallRequiredRules()
        {
            bool status = true;
            foreach(FirewallRule rule in allRules)
            {
                if (!rule.Apply(true))
                {
                    status = false;
                }
            }
            return status;
        }
        public bool RemoveInstalledRules()
        {
            bool status = true;
            foreach(FirewallRule rule in allRules)
            {
                if (!rule.Apply(false))
                {
                    status = false;
                }
            }
            return status;
        }
    }
}


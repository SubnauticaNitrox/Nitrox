using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace InstallerActions.Patches
{
    public enum FirewallRuleType
    {
        PortsUDP=0,
        ProgramServer=1,
        ProgramClient=2
    }
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
        public FirewallRuleType ruleType;
        public string addCommand;
        public string removeCommand;
        public bool isInstalled;
        string[] GetAddRemoveCommand(FirewallRuleType RuleType, string GamePath, int Port = 11000)
        {
            string[] commands = new string[2] { "", "" };
            if (RuleType == FirewallRuleType.PortsUDP)
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod UDP\x22 dir=in action=allow protocol=UDP localport=" + Port;
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod UDP\x22 protocol=UDP localport=" + Port;
            }
            else if (RuleType == FirewallRuleType.ProgramServer)
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Server\x22 dir=in action=allow program=\x22" + Path.Combine(GamePath, "SubServer", "NitroxServer.exe") + "\x22 enable=yes";
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Server\x22";
            }
            else if (RuleType == FirewallRuleType.ProgramClient)
            {
                commands[0] = "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Client\x22 dir=in action=allow program=\x22" + Path.Combine(GamePath, "Subnautica.exe") + "\x22 enable=yes";
                commands[1] = "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Client\x22";
            }
            return commands;
        }
        bool GetRuleInstalled(FirewallRuleType RuleType)
        {
            string command = "", output = "";
            if (RuleType == FirewallRuleType.PortsUDP)
            {
                command = "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod UDP\x22";
            }
            else if (RuleType == FirewallRuleType.ProgramServer)
            {
                command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Server\x22";
            }
            else if (RuleType == FirewallRuleType.ProgramClient)
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
        public bool CreateRule(FirewallRuleType NewRuleType, string GamePath, int Port = 11000)
        {
            try
            {
                isInstalled = GetRuleInstalled(NewRuleType);
                string[] commands = GetAddRemoveCommand(NewRuleType, GamePath, Port);
                addCommand = commands[0];
                removeCommand = commands[1];
                ruleType=NewRuleType;
                return true;
            }
            catch(Exception ex)
            {
                return false;
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
            FirewallRuleType[] ruleTypes = new FirewallRuleType[3] { FirewallRuleType.PortsUDP, FirewallRuleType.ProgramServer, FirewallRuleType.ProgramClient};
            foreach(FirewallRuleType eachType in ruleTypes)
            {
                FirewallRule rule = new FirewallRule();
                if(!rule.CreateRule(eachType, subPath, portNumber))
                {
                    MessageBox.Show("Error in modifying Windows Firewall. You may have to add some exceptions manually");
                }
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



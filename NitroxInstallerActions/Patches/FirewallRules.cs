using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace InstallerActions.Patches
{
    public class FirewallRules
    {
        string subPath;
        int portNumber;
        public FirewallRules(string GamePath, int PortNumber=11000)
        {
            subPath = GamePath;
            portNumber = PortNumber;
        }
        public bool InstallRequiredRules()
        {
            List<string> requiredRules = GetRequiredRules();
            bool status = SetFirewallRules(requiredRules, subPath, portNumber);
            return status;
        }
        public bool RemoveInstalledRules()
        {
            List<string> installedRules = GetInstalledRules();
            bool status = RemoveFirewallRules(installedRules, portNumber);
            return status;
        }
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
        bool SetFirewallRules(List<string> RulesArray, string SubPath, int PortNumber)
        {
            bool status = true;
            foreach(string item in RulesArray)
            {
                string command = "nothing";
                if (item == "PortsUDP")
                {
                    command= "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod UDP\x22 dir=in action=allow protocol=UDP localport=" + PortNumber;
                }
                if (item == "ProgramServer")
                {
                    command= "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Server\x22 dir=in action=allow program=\x22" + Path.Combine(SubPath, "SubServer", "NitroxServer.exe") + "\x22 enable=yes";
                }
                if (item == "ProgramClient")
                {
                    command= "advfirewall firewall add rule name=\x22Nitrox Multiplayer Mod Client\x22 dir=in action=allow program=\x22" + Path.Combine(SubPath, "Subnautica.exe") + "\x22 enable=yes";
                }
                if (!(command == "nothing"))
                {
                    string output = RunNetshCommand(command);
                    if (!output.Contains("Ok."))
                    {
                        status = false;
                    }
                }
            }
            return status;
        }
        bool RemoveFirewallRules(List<string> RulesArray, int PortNumber)
        {
            bool status=true;
            foreach(string item in RulesArray)
            {
                string command = "nothing";
                if (item == "PortsUDP")
                {
                    command= "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod UDP\x22 protocol=UDP localport=" + PortNumber;
                }
                if (item == "ProgramServer")
                {
                    command= "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Server\x22";
                }
                if (item == "ProgramClient")
                {
                    command= "advfirewall firewall delete rule name=\x22Nitrox Multiplayer Mod Client\x22";
                }
                if (!(command == "nothing"))
                {
                    string output = RunNetshCommand(command);
                    if (!output.Contains("Ok."))
                    {
                        status = false;
                    }
                }
            }
            return status;
        }
        List<string> GetInstalledRules()
        {
            List<string> installedRules = new List<string>();
            string command="", output="";
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod UDP\x22";
            output = RunNetshCommand(command);
            if (!output.Contains("No rules match the specified criteria"))
            {
                installedRules.Add("PortsUDP");
            }
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Server\x22";
            output = RunNetshCommand(command);
            if(!output.Contains("No rules match the specified criteria"))
            {
                installedRules.Add("ProgramServer");
            }
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Client\x22";
            output = RunNetshCommand(command);
            if(!output.Contains("No rules match the specified criteria"))
            {
                installedRules.Add("ProgramClient");
            }
            return installedRules;
        }

        List<string> GetRequiredRules()
        {
            List<string> requiredNitroxRules=new List<string>();
            string command = "", output = "";
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod UDP\x22";
            output = RunNetshCommand(command);
            if(output.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules.Add("PortsUDP");
            }
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Server\x22";
            output = RunNetshCommand(command);
            if(output.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules.Add("ProgramServer");
            }
            command= "advfirewall firewall show rule name=\x22Nitrox Multiplayer Mod Client\x22";
            output = RunNetshCommand(command);
            if(output.Contains("No rules match the specified criteria"))
            {
                requiredNitroxRules.Add("ProgramClient");
            }
            return requiredNitroxRules;
        }
    }
}


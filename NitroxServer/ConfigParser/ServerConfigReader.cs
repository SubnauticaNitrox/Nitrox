using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NitroxServer.ConfigParser
{
    public class ServerConfigReader
    {
        public int serverPort;
        public int maxConn;
        public void ReadServerConfig(string configPath)
        {
            string INIContents;
            if (File.Exists(configPath))
            {
                INIContents = File.ReadAllText(configPath);
                Char Splitter = ';';
                Char equals = '=';
                string[] SplitINIContents = INIContents.Split(Splitter);
                string[] ParseINIContents;
                ParseINIContents = SplitINIContents[0].Split(equals);
                serverPort = int.Parse(ParseINIContents[1]);
                ParseINIContents = SplitINIContents[1].Split(equals);
                maxConn = int.Parse(ParseINIContents[1]);
                return;
            }
            else
            {
                string NewINIContents;
                NewINIContents = "[NetworkSettings]\nDefaultPortNumber=11000;\nMaxConnections=100;";
                File.WriteAllText(configPath, NewINIContents);
                serverPort = 11000;
                maxConn = 100;
                return;
            }
        }
    }
}

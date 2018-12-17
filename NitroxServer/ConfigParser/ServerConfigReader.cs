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
        private bool gotServerPort = false;
        private bool gotMaxConn = false;
        public void ReadServerConfig(string configPath)
        {
            string INIContents;
            if (File.Exists(configPath))
            {
                INIContents = File.ReadAllText(configPath);
                Char Splitter = ';';
                Char equals = '=';
                string[] SplitINIContents = INIContents.Split(Splitter);
                foreach(string splitConfig in SplitINIContents)
                {
                    if (splitConfig.Contains("DefaultPortNumber"))
                    {
                        string[] parseINIContents;
                        parseINIContents = splitConfig.Split(equals);
                        serverPort = int.Parse(parseINIContents[1]);
                        gotServerPort = true;
                    }
                    if (splitConfig.Contains("MaxConnections"))
                    {
                        string[] parseINIContents;
                        parseINIContents = splitConfig.Split(equals);
                        maxConn = int.Parse(parseINIContents[1]);
                        gotMaxConn = true;
                    }
                }
                if (gotMaxConn == false)
                {
                    maxConn = 100;
                }
                if (gotServerPort == false)
                {
                    serverPort = 11000;
                }
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

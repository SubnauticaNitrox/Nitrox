using System;
using System.IO;
using NitroxClient.Persistence.Model;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using LitJson;

namespace NitroxClient.Persistence
{
    public static class PersistedClientData
    {
        private static PersistedClientDataModel model = null;

        private const string FILE_NAME = ".\\client.json";

        private static void CheckSingleton()
        {
            if (model == null)
            {
                string text = File.ReadAllText(FILE_NAME);
                model = JsonMapper.ToObject<PersistedClientDataModel>(text);
            }
        }

        public static Guid GetAuthToken(string ip, string port)
        {
            CheckSingleton();
            foreach (SavedServer pair in model.SavedServers)
            {
                if (pair.Ip.Equals(ip) && pair.Port.Equals(port)) //Always use trusted members to the left a .Equals call. Don't compare potentially unsafe data; assume parameters might be null.
                {
                    return new Guid(pair.AuthToken);
                }
            }
            return Guid.Empty;
        }

        public static void EmplaceServer(string name, string ip, string port, Guid authToken)
        {
            CheckSingleton();
            SavedServer savedServer = new SavedServer() { Name = name, Ip = ip, Port = port, AuthToken = authToken.ToString() };
            if(GetAuthToken(savedServer.Ip, savedServer.Port) != Guid.Empty) //Some validation is better than no validation.
            {
                throw new Exception($"Failed to add '{ip}':'{port}' because there is already an entry for it.");
            }
            model.SavedServers.Add(savedServer);
            File.WriteAllText(FILE_NAME, JsonMapper.ToJson(model));
        }

        public static List<SavedServer> GetServers()
        {
            CheckSingleton();
            return model.SavedServers;
        }

        public static void InitalizeServerList()
        {
            if (!File.Exists(FILE_NAME))
            {
                model = new PersistedClientDataModel();
                //Remove this regression scenario in later releases.
                if(File.Exists(".\\servers"))
                {
                    using (StreamReader sr = new StreamReader(".\\servers"))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            string[] lineData = line.Split('|');
                            string serverName = lineData[0];
                            string serverIp = lineData[1];
                            string serverPort;
                            if (lineData.Length == 3)
                            {
                                serverPort = lineData[2];
                            }
                            else
                            {
                                Match match = Regex.Match(serverIp, @"^(.*?)(?::(\d{3,5}))?$");
                                serverIp = match.Groups[1].Value;
                                serverPort = match.Groups[2].Success ? match.Groups[2].Value : "11000";
                            }
                            model.SavedServers.Add(new SavedServer() { Name = serverName, Ip = serverIp, Port = serverPort, AuthToken = Guid.NewGuid().ToString() });
                        }
                    }
                    File.Delete(".\\servers");
                }
                else
                {
                    model.SavedServers.Add(new SavedServer() { Name = "local server", Ip = "127.0.0.1", Port = "11000", AuthToken = Guid.NewGuid().ToString() });
                }
                File.WriteAllText(FILE_NAME, JsonMapper.ToJson(model));
            }
        }

        public static void RemoveServerByIndex(int index)
        {
            CheckSingleton();
            if(model.SavedServers.Count > index)
            {
                model.SavedServers.RemoveAt(index);
                File.WriteAllText(FILE_NAME, JsonMapper.ToJson(model));
            }
        }
    }
}

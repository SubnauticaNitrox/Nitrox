using System;
using System.Configuration;
using NitroxModel.MultiplayerSession;

namespace NitroxServer.ConfigParser
{
    public class ServerConfig
    {
        private const int MAX_CONNECTIONS = 100;
        private const string MAX_CONNECTIONS_SETTING = "MaxPlayers";
        private const int DEFAULT_SERVER_PORT = 11000;
        private const string DEFAULT_SERVER_PORT_SETTING = "Port";
        private const bool DEFAULT_ALLOW_CHEATS = false;
        private const string DEFAULT_ALLOW_CHEATS_SETTING = "AllowCheats";

        private bool? _allowCheats = null;
        public bool AllowCheats
        {
            get
            {
                bool configValue;
                if (_allowCheats == null && bool.TryParse(ConfigurationManager.AppSettings[DEFAULT_ALLOW_CHEATS_SETTING], out configValue))
                {
                    _allowCheats = configValue;
                }
                return _allowCheats ?? DEFAULT_ALLOW_CHEATS;
            }
        }

        private int? _serverPort = null;
        public int ServerPort
        {
            get
            {
                int configValue;
                if (_serverPort == null && int.TryParse(ConfigurationManager.AppSettings[DEFAULT_SERVER_PORT_SETTING], out configValue))
                {
                    _serverPort = configValue;
                }
                return _serverPort ?? DEFAULT_SERVER_PORT;
            }
        }

        private int? _maxConnections = null;
        public int MaxConnections
        {
            get
            {
                int configValue;
                if (_maxConnections == null && int.TryParse(ConfigurationManager.AppSettings[MAX_CONNECTIONS_SETTING], out configValue))
                {
                    _maxConnections = configValue;
                }
                return _maxConnections ?? MAX_CONNECTIONS;
            }
        }
    }
}

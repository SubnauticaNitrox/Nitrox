using System;
using System.Configuration;
using NitroxModel.MultiplayerSession;

namespace NitroxServer.ConfigParser
{
    public class ServerConfig
    {
        private const int MAX_CONNECTIONS = 100;
        private const string MAX_CONNECTIONS_SETTING = "MaxConnections";
        private const int DEFAULT_SERVER_PORT = 11000;
        private const string DEFAULT_SERVER_PORT_SETTING = "DefaultPortNumber";
        private const int DEFAULT_SAVE_INTERVAL = 60000;
        private const string DEFAULT_SAVE_SETTING = "SaveInterval";

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

        private int? _saveInterval = null;
        public int SaveInterval
        {
            get
            {
                int configValue;
                if (_saveInterval == null && Int32.TryParse(ConfigurationManager.AppSettings[DEFAULT_SAVE_SETTING], out configValue))
                {
                    _saveInterval = configValue;
                }
                return _saveInterval ?? DEFAULT_SAVE_INTERVAL;
            }
        }
    }
}

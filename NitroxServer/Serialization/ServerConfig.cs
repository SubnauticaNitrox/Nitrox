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
        private const string DEFAULT_SERVER_PASSWORD = "yourpassword";
        private const string DEFAULT_PASSWORD_SETTING = "ServerAdminPassword";

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
                return _saveInterval * 1000 ?? DEFAULT_SAVE_INTERVAL;
            }
        }

        public string ServerAdminPassword
        {
            get
            {
                string _ServerAdminPassword = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings[DEFAULT_PASSWORD_SETTING]))
                {
                    _ServerAdminPassword = ConfigurationManager.AppSettings[DEFAULT_PASSWORD_SETTING];
                }
                return _ServerAdminPassword ?? DEFAULT_SERVER_PASSWORD;
            }
        }
    }
}

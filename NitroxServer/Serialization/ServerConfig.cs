using System;
using System.Configuration;
using System.Collections.Generic;
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
        private const GameModeOption DEFAULT_GAMEMODE = GameModeOption.Survival;
        private const string GAMEMODE_SETTING = "GameMode";
        private const bool DEFAULT_DISABLECONSOLE = true;
        private const string DISABLECONSOLE_SETTING = "DisableConsole";

        private Dictionary<string, GameModeOption> gameModeByConfig = new Dictionary<string, GameModeOption>
        {
            {"survival", GameModeOption.Survival},
            {"creative", GameModeOption.Creative},
            {"hardcore", GameModeOption.Hardcore},
            {"permadeath", GameModeOption.Permadeath},
            {"freedom", GameModeOption.Freedom},
        };

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

        private bool? _disableConsole = null;
        public bool DisableConsole
        {
            get
            {
                bool configValue;
                if (_disableConsole == null && bool.TryParse(ConfigurationManager.AppSettings[DISABLECONSOLE_SETTING], out configValue))
                {
                    _disableConsole = configValue;
                }
                return _disableConsole ?? DEFAULT_DISABLECONSOLE;
            }
        }

        private GameModeOption? _gameMode = null;
        public GameModeOption GameMode
        {
            get
            {
                if (_gameMode == null && ConfigurationManager.AppSettings[GAMEMODE_SETTING] != null)
                {
                    _gameMode = ParseGameMode(ConfigurationManager.AppSettings[GAMEMODE_SETTING]);
                }
                return _gameMode ?? DEFAULT_GAMEMODE;
            }
        }

        private GameModeOption ParseGameMode(string stringGameMode)
        {
            GameModeOption gameMode = GameModeOption.Survival;
            stringGameMode = stringGameMode.ToLower(); // Lets be frank people have habits

            gameModeByConfig.TryGetValue(stringGameMode, out gameMode);
            return gameMode;
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

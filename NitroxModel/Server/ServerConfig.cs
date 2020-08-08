using System;
using System.ComponentModel;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxModel.Server
{
    public class ServerConfig
    {
        private readonly ServerConfigItem<ServerGameMode> gameModeSetting;
        private readonly ServerConfigItem<ServerSerializerMode> serverSerializerModeSetting;
        private readonly ServerConfigItem<bool> disableConsoleSetting, disableAutoSaveSetting;
        private readonly ServerConfigItem<int> portSetting, saveIntervalSetting, maxConnectionsSetting;
        private readonly ServerConfigItem<string> saveNameSetting, serverPasswordSetting, adminPasswordSetting;
        private readonly ServerConfigItem<float> oxygenSetting, maxOxygenSetting, healthSetting, foodSetting, waterSetting, infectionSetting;

        public ServerConfig() : this(
               port: 1100,
               saveInterval: 120000,
               maxConnection: 100,
               disableConsole: false,
               disableAutoSave: false,
               saveName: "world",
               serverPassword: string.Empty,
               adminPassword: StringHelper.GenerateRandomString(12),
               gameModeSetting: ServerGameMode.SURVIVAL,
               serverSerializerMode: ServerSerializerMode.PROTOBUF
        )
        { }

        public ServerConfig(int port, int saveInterval, int maxConnection, bool disableConsole, bool disableAutoSave, string saveName, string serverPassword, string adminPassword, ServerGameMode gameModeSetting, ServerSerializerMode serverSerializerMode)
        {
            portSetting = new ServerConfigItem<int>("Port", port);
            saveIntervalSetting = new ServerConfigItem<int>("SaveInterval", saveInterval);
            maxConnectionsSetting = new ServerConfigItem<int>("MaxConnections", maxConnection);
            disableConsoleSetting = new ServerConfigItem<bool>("DisableConsole", disableConsole);
            disableAutoSaveSetting = new ServerConfigItem<bool>("DisableAutoSave", disableAutoSave);
            saveNameSetting = new ServerConfigItem<string>("SaveName", saveName);
            serverPasswordSetting = new ServerConfigItem<string>("ServerPassword", serverPassword);
            adminPasswordSetting = new ServerConfigItem<string>("AdminPassword", adminPassword);
            this.gameModeSetting = new ServerConfigItem<ServerGameMode>("GameMode", gameModeSetting);
            serverSerializerModeSetting = new ServerConfigItem<ServerSerializerMode>("SaveFileSerializer", serverSerializerMode);

            //We don't want to custom those values for now
            oxygenSetting = new ServerConfigItem<float>("StartOxygen", 45);
            maxOxygenSetting = new ServerConfigItem<float>("StartMaxOxygen", 45);
            healthSetting = new ServerConfigItem<float>("StartHealth", 80);
            foodSetting = new ServerConfigItem<float>("StartFood", 50.5f);
            waterSetting = new ServerConfigItem<float>("StartWater", 90.5f);
            infectionSetting = new ServerConfigItem<float>("StartInfection", 0);
        }

        #region Properties
        public int ServerPort
        {
            get => portSetting.Value;

            set
            {
                Validate.IsTrue(value > 1024, "Server Port must be greater than 1024");
                portSetting.Value = value;
            }
        }

        public int SaveInterval
        {
            get => saveIntervalSetting.Value;

            set
            {
                Validate.IsTrue(value > 1000, "SaveInterval must be greater than 1000");
                saveIntervalSetting.Value = value;
            }
        }

        public int MaxConnections
        {
            get => maxConnectionsSetting.Value;

            set
            {
                Validate.IsTrue(value > 0, "MaxConnections must be greater than 0");
                maxConnectionsSetting.Value = value;
            }
        }

        public bool DisableConsole
        {
            get => disableConsoleSetting.Value;

            set => disableConsoleSetting.Value = value;
        }

        public bool DisableAutoSave
        {
            get => disableAutoSaveSetting.Value;

            set => disableAutoSaveSetting.Value = value;
        }

        public string SaveName
        {
            get => saveNameSetting.Value;

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "SaveName can't be an empty string");
                saveNameSetting.Value = value;
            }
        }

        public string ServerPassword
        {
            get => serverPasswordSetting.Value;

            set
            {
                Validate.NotNull(value);
                serverPasswordSetting.Value = value;
            }
        }

        public string AdminPassword
        {
            get => adminPasswordSetting.Value;

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "AdminPassword can't be an empty string");
                adminPasswordSetting.Value = value;
            }
        }

        public ServerGameMode GameModeEnum
        {
            get => gameModeSetting.Value;

            private set
            {
                gameModeSetting.Value = value;
            }
        }

        public ServerSerializerMode SerializerModeEnum
        {
            get
            {
                return serverSerializerModeSetting.Value;
            }

            set
            {
                serverSerializerModeSetting.Value = value;
            }
        }

        public PlayerStatsData DefaultPlayerStats => new PlayerStatsData(oxygenSetting.Value, maxOxygenSetting.Value, healthSetting.Value, foodSetting.Value, waterSetting.Value, infectionSetting.Value);
        #endregion

        public bool IsHardcore => GameModeEnum == ServerGameMode.HARDCORE;
    }
}

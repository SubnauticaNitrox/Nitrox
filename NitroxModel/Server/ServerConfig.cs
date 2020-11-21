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
               port: 11000,
               saveinterval: 120000,
               maxconnection: 100,
               disableconsole: false,
               disableautosave: false,
               savename: "world",
               serverpassword: string.Empty,
               adminpassword: StringHelper.GenerateRandomString(12),
               gamemode: ServerGameMode.SURVIVAL,
               serverserializermode: ServerSerializerMode.PROTOBUF
        )
        { }

        public ServerConfig(int port, int saveinterval, int maxconnection, bool disableconsole, bool disableautosave, string savename, string serverpassword, string adminpassword, ServerGameMode gamemode, ServerSerializerMode serverserializermode)
        {
            portSetting = new ServerConfigItem<int>("Port", port);
            saveIntervalSetting = new ServerConfigItem<int>("SaveInterval", saveinterval);
            maxConnectionsSetting = new ServerConfigItem<int>("MaxConnections", maxconnection);
            disableConsoleSetting = new ServerConfigItem<bool>("DisableConsole", disableconsole);
            disableAutoSaveSetting = new ServerConfigItem<bool>("DisableAutoSave", disableautosave);
            saveNameSetting = new ServerConfigItem<string>("SaveName", savename);
            serverPasswordSetting = new ServerConfigItem<string>("ServerPassword", serverpassword);
            adminPasswordSetting = new ServerConfigItem<string>("AdminPassword", adminpassword);
            gameModeSetting = new ServerConfigItem<ServerGameMode>("GameMode", gamemode);
            serverSerializerModeSetting = new ServerConfigItem<ServerSerializerMode>("SaveFileSerializer", serverserializermode);

            //We don't want to custom those values for now
            oxygenSetting = new ServerConfigItem<float>("StartOxygen", 45);
            maxOxygenSetting = new ServerConfigItem<float>("StartMaxOxygen", 45);
            healthSetting = new ServerConfigItem<float>("StartHealth", 80);
            foodSetting = new ServerConfigItem<float>("StartFood", 50.5f);
            waterSetting = new ServerConfigItem<float>("StartWater", 90.5f);
            //Infection level of 0f will make it so everyone starts the game cured. 0.1f is the default starting value. Also set in NitroxServer-Subnautica App.config
            infectionSetting = new ServerConfigItem<float>("StartInfection", 0.1f);
        }

        #region Properties
        public int ServerPort
        {
            get
            {
                return portSetting.Value;
            }

            set
            {
                Validate.IsTrue(value > 1024, "Server Port must be greater than 1024");
                portSetting.Value = value;
            }
        }

        public int SaveInterval
        {
            get
            {
                return saveIntervalSetting.Value;
            }

            set
            {
                Validate.IsTrue(value > 1000, "SaveInterval must be greater than 1000");
                saveIntervalSetting.Value = value;
            }
        }

        public int MaxConnections
        {
            get
            {
                return maxConnectionsSetting.Value;
            }

            set
            {
                Validate.IsTrue(value > 0, "MaxConnections must be greater than 0");
                maxConnectionsSetting.Value = value;
            }
        }

        public bool DisableConsole
        {
            get
            {
                return disableConsoleSetting.Value;
            }

            set
            {
                disableConsoleSetting.Value = value;
            }
        }

        public bool DisableAutoSave
        {
            get
            {
                return disableAutoSaveSetting.Value;
            }

            set
            {
                disableAutoSaveSetting.Value = value;
            }
        }

        public string SaveName
        {
            get
            {
                return saveNameSetting.Value;
            }

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "SaveName can't be an empty string");
                saveNameSetting.Value = value;
            }
        }

        public string ServerPassword
        {
            get
            {
                return serverPasswordSetting.Value;
            }

            set
            {
                Validate.NotNull(value);
                serverPasswordSetting.Value = value;
            }
        }

        public string AdminPassword
        {
            get
            {
                return adminPasswordSetting.Value;
            }

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "AdminPassword can't be an empty string");
                adminPasswordSetting.Value = value;
            }
        }

        public ServerGameMode GameModeEnum
        {
            get
            {
                return gameModeSetting.Value;
            }

            set
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

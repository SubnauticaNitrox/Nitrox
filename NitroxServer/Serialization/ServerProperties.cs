using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxModel.Serialization;

namespace NitroxServer.Serialization
{
    [PropertyDescription("Server settings can be changed here")]
    public class ServerProperties : IProperties
    {
        public string FileName => "config.properties";

        public int ServerPort
        {
            get
            {
                return portSetting;
            }

            set
            {
                Validate.IsTrue(value > 1024, "Server Port must be greater than 1024");
                portSetting = value;
            }
        }

        private int portSetting = 11000;

        [PropertyDescription("Measured in milliseconds")]
        public int SaveInterval
        {
            get
            {
                return saveIntervalSetting;
            }

            set
            {
                Validate.IsTrue(value > 1000, "SaveInterval must be greater than 1000");
                saveIntervalSetting = value;
            }
        }

        private int saveIntervalSetting = 120000;

        public int MaxConnections
        {
            get
            {
                return maxConnectionsSetting;
            }

            set
            {
                Validate.IsTrue(value > 0, "MaxConnections must be greater than 0");
                maxConnectionsSetting = value;
            }
        }

        private int maxConnectionsSetting = 100;


        public bool DisableConsole { get; set; }

        public bool DisableAutoSave { get; set; }

        public string SaveName
        {
            get
            {
                return saveNameSetting;
            }

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "SaveName can't be an empty string");
                saveNameSetting = value;
            }
        }

        private string saveNameSetting = "world";

        public string ServerPassword { get; set; } = string.Empty;

        public string AdminPassword { get; set; } = StringHelper.GenerateRandomString(12);

        [PropertyDescription("Possible values:", typeof(ServerGameMode))]
        public ServerGameMode GameMode { get; set; } = ServerGameMode.SURVIVAL;

        [PropertyDescription("Possible values:", typeof(ServerSerializerMode))]
        public ServerSerializerMode SerializerMode { get; set; } = ServerSerializerMode.PROTOBUF;

        [PropertyDescription("\nDefault player stats below here")]
        public float DefaultOxygenValue { get; set; } = 45;
        public float DefaultMaxOxygenValue { get; set; } = 45;
        public float DefaultHealthValue { get; set; } = 80;
        public float DefaultHungerValue { get; set; } = 50.5f;
        public float DefaultThirstValue { get; set; } = 90.5f;
        public float DefaultInfectionValue { get; set; } = 0;
        public bool IsHardcore => GameMode == ServerGameMode.HARDCORE;
        public PlayerStatsData DefaultPlayerStats => new PlayerStatsData(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);
    }
}

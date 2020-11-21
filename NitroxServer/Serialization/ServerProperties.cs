using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace NitroxServer.Serialization
{
    [PropertyDescription("Server settings can be changed here")]
    public class ServerProperties : IProperties
    {
        private int maxConnectionsSetting = 100;

        private int portSetting = 11000;

        private int saveIntervalSetting = 120000;

        private string saveNameSetting = "world";
        public string FileName => "config.properties";

        public int ServerPort
        {
            get => portSetting;

            set
            {
                Validate.IsTrue(value > 1024, "Server Port must be greater than 1024");
                portSetting = value;
            }
        }

        [PropertyDescription("Measured in milliseconds")]
        public int SaveInterval
        {
            get => saveIntervalSetting;

            set
            {
                Validate.IsTrue(value > 1000, "SaveInterval must be greater than 1000");
                saveIntervalSetting = value;
            }
        }

        public int MaxConnections
        {
            get => maxConnectionsSetting;

            set
            {
                Validate.IsTrue(value > 0, "MaxConnections must be greater than 0");
                maxConnectionsSetting = value;
            }
        }

        public bool DisableConsole { get; set; }

        public bool DisableAutoSave { get; set; }

        public string SaveName
        {
            get => saveNameSetting;

            set
            {
                Validate.IsFalse(string.IsNullOrWhiteSpace(value), "SaveName can't be an empty string");
                saveNameSetting = value;
            }
        }

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

        /// <summary>
        ///     Infection level of 0f will make it so everyone starts the game cured. 0.1f is the default starting value.
        /// </summary>
        public float DefaultInfectionValue { get; set; } = 0.1f;

        public bool IsHardcore => GameMode == ServerGameMode.HARDCORE;
        public PlayerStatsData DefaultPlayerStats => new PlayerStatsData(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);
    }
}

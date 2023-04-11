using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.Serialization
{
    [PropertyDescription("Server settings can be changed here")]
    public class ServerConfig : NitroxConfig<ServerConfig>
    {
        private int maxConnectionsSetting = 100;

        private int initialSyncTimeoutSetting = 300000;

        private int saveIntervalSetting = 120000;

        private string postSaveCommandPath = string.Empty;

        private string saveNameSetting = "My World";
        public override string FileName => "server.cfg";

        [PropertyDescription("Leave blank for a random spawn position")]
        public string Seed { get; set; }

        public int ServerPort { get; set; } = ServerList.DEFAULT_PORT;

        [PropertyDescription("Measured in milliseconds")]
        public int SaveInterval
        {
            get => saveIntervalSetting;

            set
            {
                Validate.IsTrue(value >= 1000, "SaveInterval must be greater than 1000");
                saveIntervalSetting = value;
            }
        }

        [PropertyDescription("Command to run following a successful world save (e.g. .exe, .bat, or PowerShell script). ")]
        public string PostSaveCommandPath
        {
            get => postSaveCommandPath;
            set => postSaveCommandPath = value?.Trim('"').Trim();
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

        public int InitialSyncTimeout
        {
            get => initialSyncTimeoutSetting;

            set
            {
                Validate.IsTrue(value > 30000, "InitialSyncTimeout must be greater than 30 seconds");
                initialSyncTimeoutSetting = value;
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
        public ServerSerializerMode SerializerMode { get; set; } = ServerSerializerMode.JSON;

        [PropertyDescription("Possible values:", typeof(Perms))]
        public Perms DefaultPlayerPerm { get; set; } = Perms.PLAYER;

        [PropertyDescription("\nDefault player stats below here")]
        public float DefaultOxygenValue { get; set; } = 45;

        public float DefaultMaxOxygenValue { get; set; } = 45;
        public float DefaultHealthValue { get; set; } = 80;
        public float DefaultHungerValue { get; set; } = 50.5f;
        public float DefaultThirstValue { get; set; } = 90.5f;

        [PropertyDescription("Recommended to keep at 0.1f which is the default starting value. If set to 0 then new players are cured by default.")]
        public float DefaultInfectionValue { get; set; } = 0.1f;

        public bool IsHardcore => GameMode == ServerGameMode.HARDCORE;
        public bool IsPasswordRequired => ServerPassword != string.Empty;
        public PlayerStatsData DefaultPlayerStats => new(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);
        [PropertyDescription("If set to true, the server will try to open port on your router via UPnP")]
        public bool AutoPortForward { get; set; } = true;
        [PropertyDescription("Determines whether the server will listen for and reply to LAN discovery requests.")]
        public bool LANDiscoveryEnabled { get; set; } = true;

        [PropertyDescription("Determines how the server spawns entities in the world.", typeof(SpawnMode))]
        public SpawnMode SpawnMode =
#if DEBUG
            SpawnMode.STREAMING; // For development, baked and upfront can be hard on the SSD/HDD when creating many worlds over and over.
#else
            SpawnMode.BAKED; // For the release build, we want users to generally use baked so they have instant loading.
#endif

        [PropertyDescription("Name of the save to fetch the prebaked nitrox repo (only applicable when SpawnMode = BAKED)", typeof(SpawnMode))]
        public string prebakedSave = "Prebaked-v1.zip";
    }
}

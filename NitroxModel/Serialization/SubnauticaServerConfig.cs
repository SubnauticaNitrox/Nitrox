using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;

namespace NitroxModel.Serialization
{
    [PropertyDescription("Server settings can be changed here")]
    public class SubnauticaServerConfig : NitroxConfig<SubnauticaServerConfig>
    {
        private int maxConnectionsSetting = 100;

        private int initialSyncTimeoutSetting = 120000;

        [PropertyDescription("Set to true to Cache entities for the whole map on next run. \nWARNING! Will make server load take longer on the cache run but players will gain a performance boost when entering new areas.")]
        public bool CreateFullEntityCache { get; set; } = false;

        private int saveIntervalSetting = 120000;

        private int maxBackupsSetting = 10;

        private string postSaveCommandPath = string.Empty;

        public override string FileName => "server.cfg";

        [PropertyDescription("Leave blank for a random spawn position")]
        public string Seed { get; set; }

        public int ServerPort { get; set; } = ServerList.DEFAULT_PORT;

        [PropertyDescription("Prevents players from losing items on death")]
        public bool KeepInventoryOnDeath { get; set; } = false;

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

        public int MaxBackups
        {
            get => maxBackupsSetting;

            set
            {
                Validate.IsTrue(value >= 0, "MaxBackups must be greater than or equal to 0");
                maxBackupsSetting = value;
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

        [PropertyDescription("Measured in milliseconds")]
        public int InitialSyncTimeout
        {
            get => initialSyncTimeoutSetting;

            set
            {
                initialSyncTimeoutSetting = value;
            }
        }

        public bool DisableConsole { get; set; }

        public bool DisableAutoSave { get; set; }

        public bool DisableAutoBackup { get; set; }

        public string ServerPassword { get; set; } = string.Empty;

        public string AdminPassword { get; set; } = StringHelper.GenerateRandomString(12);

        [PropertyDescription("Possible values:", typeof(NitroxGameMode))]
        public NitroxGameMode GameMode { get; set; } = NitroxGameMode.SURVIVAL;

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

        public PlayerStatsData DefaultPlayerStats => new(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);
        [PropertyDescription("If set to true, the server will try to open port on your router via UPnP")]
        public bool AutoPortForward { get; set; } = true;
        [PropertyDescription("Determines whether the server will listen for and reply to LAN discovery requests.")]
        public bool LANDiscoveryEnabled { get; set; } = true;

        [PropertyDescription("When true, will reject any build actions detected as desynced")]
        public bool SafeBuilding { get; set; } = true;

        [PropertyDescription("When true and started in launcher, will use launcher UI as opposed to external window")]
        public bool IsEmbedded { get; set; } = true;

        [PropertyDescription("Activates/Deactivates Player versus Player damage/interactions")]
        public bool PvPEnabled { get; set; } = true;
    }
}

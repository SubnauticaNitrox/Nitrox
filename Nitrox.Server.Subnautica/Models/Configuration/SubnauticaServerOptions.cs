using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Configuration;

/// <summary>
///     Options that are tied to a hosted game world.
/// </summary>
public sealed partial class SubnauticaServerOptions
{
    public const string CONFIG_SECTION_PATH = "GameServer";

    [Range(1, byte.MaxValue)]
    public byte MaxConnections { get; set; } = 100;

    [ConfigurationKeyName("port")]
    [Range(1, ushort.MaxValue)]
    public ushort ServerPort { get; set; } = 11000;

    [RegularExpression(@"\w+")]
    public string ServerPassword { get; set; } = "";

    public SubnauticaGameMode GameMode { get; set; }

    [Range(30001, int.MaxValue)]
    public int InitialSyncTimeout { get; set; } = 300000;

    public bool IsHardcore => GameMode == SubnauticaGameMode.HARDCORE;

    [PropertyDescription("Possible values:", typeof(Perms))]
    public Perms DefaultPlayerPerm { get; set; } = Perms.DEFAULT;

    public float DefaultOxygenValue { get; set; } = 45;

    public float DefaultMaxOxygenValue { get; set; } = 45;
    public float DefaultHealthValue { get; set; } = 80;
    public float DefaultHungerValue { get; set; } = 50.5f;
    public float DefaultThirstValue { get; set; } = 90.5f;

    [PropertyDescription("Recommended to keep at 0.1 which is the default starting value. If set to 0, new players are cured by default.")]
    public float DefaultInfectionValue { get; set; } = 0.1f;

    [PropertyDescription("If set to true, the server will try to open port on your router via UPnP")]
    public bool AutoPortForward { get; set; } = true;

    [PropertyDescription("Determines whether the server will listen for and reply to LAN discovery requests.")]
    public bool LanDiscoveryEnabled { get; set; } = true;

    public PlayerStatsData DefaultPlayerStats => new(DefaultOxygenValue, DefaultMaxOxygenValue, DefaultHealthValue, DefaultHungerValue, DefaultThirstValue, DefaultInfectionValue);

    [Required]
    [RegularExpression(@"\w+")]
    public string Seed { get; set; }

    public bool DisableConsole { get; set; }
    public string AdminPassword { get; set; }
    public bool KeepInventoryOnDeath { get; set; }
    public bool PvpEnabled { get; set; } = true;

    [OptionsValidator]
    internal partial class Validator : IValidateOptions<SubnauticaServerOptions>;
}

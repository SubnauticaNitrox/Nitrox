using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;

namespace Nitrox.Model.Configuration;

/// <summary>
///     Options that are tied to a hosted Subnautica world.
/// </summary>
[PropertyDescription("Server settings can be changed here")]
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

    [PropertyDescription("Measured in milliseconds. Values less than 1 second will disable auto saving.")]
    public int SaveInterval { get; set; } = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

    [Range(0, int.MaxValue)]
    public int MaxBackups { get; set; } = 10;

    [Range(30001, int.MaxValue)]
    public int InitialSyncTimeout { get; set; } = 300000;

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
    public bool PortForward { get; set; } = true;

    [PropertyDescription("Determines whether the server will listen for and reply to LAN discovery requests.")]
    public bool LanDiscovery { get; set; } = true;

    [Required]
    [RegularExpression(@"\w+")]
    public string? Seed { get; set; }

    public bool DisableConsole { get; set; }
    public string? AdminPassword { get; set; }
    public bool KeepInventoryOnDeath { get; set; }
    public bool PvpEnabled { get; set; } = true;

    [OptionsValidator]
    public partial class Validator : IValidateOptions<SubnauticaServerOptions>;
}

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Core.Configuration.Validators;

namespace Nitrox.Server.Subnautica.Models.Configuration;

/// <summary>
///     Configuration with the information needed to start a Nitrox game server.
/// </summary>
public sealed partial record ServerStartOptions
{
    [ConfigurationKeyName("save")]
    [Required]
    [SaveName]
    public string SaveName { get; set; } = "My World";

    /// <summary>
    ///     The path to the root directory containing the game files. Required for hosting a game world.
    /// </summary>
    [Required]
    [DirectoryPath]
    public string GameInstallPath { get; set; }

    [ConfigurationKeyName("embedded")]
    public bool IsEmbedded { get; set; } = !Environment.UserInteractive || Console.IsInputRedirected || Console.IsOutputRedirected;

    /// <summary>
    ///     Gets or sets the location of the Nitrox save files and cache.
    /// </summary>
    [Required]
    [DirectoryPath]
    public string NitroxAppDataPath { get; set; }

    /// <summary>
    ///     Gets or sets the location of the Nitrox assets. Defaults to the root path of the Nitrox installation.
    /// </summary>
    [Required]
    [DirectoryPath]
    public string NitroxAssetsPath { get; set; }

    [OptionsValidator]
    internal partial class Validator : IValidateOptions<ServerStartOptions>;
}

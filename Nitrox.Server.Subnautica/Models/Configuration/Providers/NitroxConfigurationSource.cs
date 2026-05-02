using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Nitrox.Server.Subnautica.Models.Configuration.Providers;

internal sealed class NitroxConfigurationSource : FileConfigurationSource
{
    /// <summary>
    ///     The config section to insert the parsed keys into.
    /// </summary>
    public required string Section { get; init; }

    [SetsRequiredMembers]
    public NitroxConfigurationSource(string path, string configSection = "", bool optional = true, IFileProvider? fileProvider = null)
    {
        Path = path;
        Section = configSection;
        Optional = optional;
        FileProvider = fileProvider;
        ReloadOnChange = true;
    }

    /// <summary>
    ///     Builds the <see cref="NitroxConfigurationProvider" /> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder" />.</param>
    /// <returns>A <see cref="NitroxConfigurationProvider" /> instance.</returns>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new NitroxConfigurationProvider(this);
    }
}

using System;

namespace Nitrox.Model.Platforms.Store.Exceptions;

/// <summary>
/// Thrown when Steam's configured Proton compatibility tool for a game is not installed on the system.
/// </summary>
public class ProtonMisconfiguredException : Exception
{
    public string ProtonVersion { get; }

    public ProtonMisconfiguredException(string protonVersion) : base($"Proton \"{protonVersion}\" is not installed.")
    {
        ProtonVersion = protonVersion;
    }
}

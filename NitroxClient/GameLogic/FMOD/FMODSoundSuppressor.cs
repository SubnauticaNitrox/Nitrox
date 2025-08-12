using System;

namespace NitroxClient.GameLogic.FMOD;

/// <summary>
/// Suppresses sounds played by base Subnautica, but not by Nitrox
/// </summary>
public readonly struct FMODSoundSuppressor : IDisposable
{
    public static bool SuppressFMODEvents { get; private set; }

    public FMODSoundSuppressor()
    {
        SuppressFMODEvents = true;
    }

    public void Dispose()
    {
        SuppressFMODEvents = false;
    }
}

namespace NitroxPatcher.Patches;

/// <summary>
///     A persistent patch is applied when the game is initializing and stays applied through the process' lifetime.
/// </summary>
public interface IPersistentPatch : INitroxPatch
{
}

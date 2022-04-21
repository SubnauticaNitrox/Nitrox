namespace NitroxPatcher.Patches;

/// <summary>
///     A dynamic patch can possibly be unloaded. For Nitrox, this should happen when the player returns to the main menu.
/// </summary>
public interface IDynamicPatch : INitroxPatch
{
}

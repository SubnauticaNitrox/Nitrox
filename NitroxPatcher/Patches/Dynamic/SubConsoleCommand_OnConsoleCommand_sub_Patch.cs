using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Called whenever a Cyclops or Seamoth is spawned. Nitrox already has its own command to spawn vehicles.
/// This patch is only meant to block the method from executing, causing two vehicles to be spawned instead of one
/// </summary>
public sealed partial class SubConsoleCommand_OnConsoleCommand_sub_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubConsoleCommand t) => t.OnConsoleCommand_sub(default));

    public static bool Prefix()
    {
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }
}

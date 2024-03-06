using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables the "decontaminate" command
/// </summary>
public sealed partial class LeakingRadiation_OnConsoleCommand_decontaminate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LeakingRadiation t) => t.OnConsoleCommand_decontaminate());

    public static bool Prefix()
    {
        // This command can't be synced because it would break how radiation leak is currently synced
        // Currently all radiation radius calculations depend on the fixed leaks
        // But this command would work even if leaks aren't fixed (modifying the radius but only on local client)
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }
}

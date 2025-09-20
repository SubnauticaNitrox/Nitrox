using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_OnConsoleCommand_randomstart_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.OnConsoleCommand_randomstart());

    public static bool Prefix()
    {
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }
}

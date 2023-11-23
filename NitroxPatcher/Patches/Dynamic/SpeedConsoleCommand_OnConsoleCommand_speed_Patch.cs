using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SpeedConsoleCommand_OnConsoleCommand_speed_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpeedConsoleCommand t) => t.OnConsoleCommand_speed(default));

    // The command is skipped because simulating speed reliable is totally out of scope
    public static bool Prefix()
    {
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }
}

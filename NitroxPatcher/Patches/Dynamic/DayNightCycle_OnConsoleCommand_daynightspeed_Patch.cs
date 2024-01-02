using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class DayNightCycle_OnConsoleCommand_daynightspeed_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((DayNightCycle t) => t.OnConsoleCommand_daynightspeed(default));

    // The command is skipped because simulating speed reliable on the server is out of scope
    public static bool Prefix()
    {
        Log.InGame(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }
}

using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class SpeedConsoleCommand_OnConsoleCommand_speed_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpeedConsoleCommand t) => t.OnConsoleCommand_speed(default));

    // The command is skipped because simulating speed reliable is totally out of scope
    public static bool Prefix()
    {
        ErrorMessage.AddMessage(Language.main.Get("Nitrox_CommandNotAvailable"));
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}

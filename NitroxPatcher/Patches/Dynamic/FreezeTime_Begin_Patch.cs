using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UWE;

/// <summary>
/// Because we're in multiplayer mode, we generally don't want the game to freeze
/// </summary>
namespace NitroxPatcher.Patches.Dynamic;

public class FreezeTime_Begin_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => FreezeTime.Begin(default(FreezeTime.Id)));

    private static HashSet<FreezeTime.Id> allowedFreezeIds = new() { FreezeTime.Id.Quit, FreezeTime.Id.WaitScreen };

    // We don't want to prevent from freezing the game if the opened modal wants to freeze the game
    public static bool Prefix(FreezeTime.Id id)
    {
        return allowedFreezeIds.Contains(id);
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}

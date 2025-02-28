using System.Collections.Generic;
using System.Reflection;
using NitroxModel.Helper;
using UWE;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Because we're in multiplayer mode, we generally don't want the game to freeze.
/// Thus we prevent any FreezeTime that is not for quit screen or (initial sync) wait screen.
/// </summary>
public sealed partial class FreezeTime_Set_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => FreezeTime.Set(default, default));

    private static readonly HashSet<FreezeTime.Id> allowedFreezeIds = [FreezeTime.Id.Quit, FreezeTime.Id.WaitScreen];

    public static bool Prefix(FreezeTime.Id id)
    {
        return allowedFreezeIds.Contains(id);
    }
}

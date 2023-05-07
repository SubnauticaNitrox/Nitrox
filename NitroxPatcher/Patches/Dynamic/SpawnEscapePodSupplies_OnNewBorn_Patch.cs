#if SUBNAUTICA
using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Don't allow the game to spawn initial supplies in the escape pod.
/// </summary>
public sealed partial class SpawnEscapePodSupplies_OnNewBorn_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SpawnEscapePodSupplies t) => t.OnNewBorn());

    public static bool Prefix()
    {
        return false;
    }
}
#endif

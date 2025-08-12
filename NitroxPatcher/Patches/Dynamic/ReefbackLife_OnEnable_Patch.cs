using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents <see cref="ReefbackLife.OnEnable"/> from starting coroutine <see cref="ReefbackLife.CoSpawn"/>.
/// Reefbacks plants and creatures are spawned by ReefbackBootstrapper and the spawning logic is in <see cref="ReefbackWorldEntitySpawner"/>
/// </summary>
public sealed partial class ReefbackLife_OnEnable_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ReefbackLife t) => t.OnEnable());

    public static bool Prefix()
    {
        return false;
    }
}

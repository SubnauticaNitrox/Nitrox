using System.Reflection;
using NitroxClient.GameLogic.Spawning;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_Awake_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.Awake());

    public static bool Prefix(EscapePod __instance)
    {
        return !EscapePodEntitySpawner.SuppressEscapePodAwakeMethod;
    }
}

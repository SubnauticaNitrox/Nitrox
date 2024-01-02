using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsMotorMode_RestoreEngineState_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsMotorMode t) => t.RestoreEngineState());

    public static bool Prefix()
    {
        // We don't want this to happen because it will prevent players that were far of the cyclops when spawning to see its actual engine state
        return false;
    }
}

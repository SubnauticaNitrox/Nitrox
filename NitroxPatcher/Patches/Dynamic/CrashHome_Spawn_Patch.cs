using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CrashHome_Spawn_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.Spawn());

    public static bool Prefix() // Disables Crashfish automatic spawning on the client
    {
        return false;
    }
}

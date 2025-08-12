using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     We don't want <see cref="CrashHome.Start"/> accidentally spawning a Crash so we prevent it from happening.
///     Instead, the spawning functionality will happen in <see cref="CrashHome.Update"/>
/// </summary>
public sealed partial class CrashHome_Start_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.Start());

    public static bool Prefix()
    {
        return false;
    }
}

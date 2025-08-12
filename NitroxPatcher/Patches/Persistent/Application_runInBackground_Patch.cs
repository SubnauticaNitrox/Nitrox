using System.Diagnostics;
using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Ensures <see cref="Application.runInBackground"/> is set to true at all times.
/// </summary>
/// <remarks>
/// Nitrox needs to be running updates and processing packets at all times to work properly.
/// </remarks>
public sealed partial class Application_runInBackground_Patch : NitroxPatch, IPersistentPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Property(() => Application.runInBackground).GetSetMethod();

    public static bool Prefix(bool value)
    {
        if (!value)
        {
            Log.WarnOnce($"An attempt to set {nameof(Application.runInBackground)} to \"false\" was issued but it was ignored.\n{new StackTrace()}");
            Application.runInBackground = true;
            return false;
        }
        return true;
    }
}

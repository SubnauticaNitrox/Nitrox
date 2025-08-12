using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
///     Once multiplayer is initiated, prevent game logic from sleeping (i.e. freezing).
/// </summary>
public sealed partial class Application_IsFocused_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Property(() => Application.isFocused).GetMethod;

    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}

using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents caching cells GameObjects.
/// </summary>
public sealed partial class EntityCell_SerializeAsyncImpl_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((EntityCell t) => t.SerializeAsyncImpl(default, default)));

    public static bool Prefix()
    {
        return false;
    }
}

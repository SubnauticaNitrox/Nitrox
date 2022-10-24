using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// When looking for an EntityRoot, we want to make sure that remote players can be recognized as one.
/// </summary>
public class Utils_GetEntityRoot_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => UWE.Utils.GetEntityRoot(default));

    public static bool Prefix(GameObject go, ref GameObject __result)
    {
        if (go.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier) || go.TryGetComponentInParent(out remotePlayerIdentifier))
        {
            __result = remotePlayerIdentifier.gameObject;
            return false;
        }
        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}

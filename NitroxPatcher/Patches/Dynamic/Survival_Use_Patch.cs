using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Let the server know when the player successfully uses a consumable item (such as a first aid kit).
/// </summary>
public class Survival_Use_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Survival t) => t.Use(default(GameObject)));

    public static void Postfix(bool __result, GameObject useObj)
    {
        if (__result && useObj.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(id));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}

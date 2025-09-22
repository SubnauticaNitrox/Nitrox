using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.StartCinematic(Player.main));

    public static void Postfix(BulkheadDoor __instance)
    {
        Log.Info("[BulkheadDoor_Patch] Postfix triggered");

        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        bool isDoorOpened = __instance.opened;
        Log.Info($"[BulkheadDoor_Patch] Door {id} state changed to: {(!isDoorOpened ? "OPEN" : "CLOSED")}");

        Resolve<IPacketSender>().Send(new BulkheadDoorStateChanged(id, isDoorOpened));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD, ((Action<BulkheadDoor>)Postfix).Method);
    }
}

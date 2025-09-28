using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.OnHandClick(default));

    public static void Postfix(BulkheadDoor __instance)
    {
        Log.Info("[BulkheadDoor_Patch] Postfix triggered");

        // Look for NitroxEntity in parent hierarchy
        if (!__instance.TryGetComponentInParent<NitroxEntity>(out NitroxEntity nitroxEntity, true))
        {
            Log.Info("[BulkheadDoor_Patch] Could not find NitroxEntity in parent hierarchy");
            return;
        }

        int instanceId = __instance.GetInstanceID();
        bool isFacingDoor = __instance.GetSide();

        Log.Info($"[BulkheadDoor_Patch] instanceId={instanceId} isFacingDoor");
        

        NitroxId id = nitroxEntity.Id;

        bool isDoorOpened = !__instance.opened;
        Log.Info($"[BulkheadDoor_Patch] Door {id} state changed to: {(isDoorOpened ? "OPEN" : "CLOSED")}");

        // get player id
        LocalPlayer player = Resolve<LocalPlayer>();

        if (player.PlayerId.HasValue)
        {
            ushort playerId = player.PlayerId.Value;
            Resolve<IPacketSender>().Send(new BulkheadDoorStateChanged(id, playerId, isDoorOpened, isFacingDoor));
        }
    }
}

using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.OnHandClick(default));

    public static void Postfix(BulkheadDoor __instance)
    {
        Log.Info("[BulkheadDoor_Patch] Postfix triggered");

        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Log.Info("Could not find NitroxId");
            return;
        }

        bool isDoorOpened = __instance.opened;
        Log.Info($"[BulkheadDoor_Patch] Door {id} state changed to: {(!isDoorOpened ? "OPEN" : "CLOSED")}");

        // get player id
        LocalPlayer player = Resolve<LocalPlayer>();

        if (player.PlayerId.HasValue)
        {
            ushort playerId = player.PlayerId.Value;
            Resolve<IPacketSender>().Send(new BulkheadDoorStateChanged(id, playerId, isDoorOpened));
        }
    }
}

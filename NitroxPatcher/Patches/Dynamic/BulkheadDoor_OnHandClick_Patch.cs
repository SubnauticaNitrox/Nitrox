using System.Reflection;
using System.Runtime.CompilerServices;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class BulkheadDoor_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BulkheadDoor t) => t.OnHandClick(default(GUIHand)));

    public static bool Prefix(BulkheadDoor __instance, GUIHand hand)
    {
        if (!__instance.TryGetComponentInParent<NitroxEntity>(out NitroxEntity nitroxEntity, true))
        {
            Log.Info("[BulkheadDoor_OnHandClick_Patch] Could not find NitroxEntity in parent hierarchy");
            return true;
        }

        NitroxId id = nitroxEntity.Id;

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"[BulkheadDoor_OnHandClick_Patch] Already have an exclusive lock on the Bulkhead door: {id}");
            return true;
        }

        HandInteraction<BulkheadDoor> context = new(__instance, hand);
        LockRequest<HandInteraction<BulkheadDoor>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, HandInteraction<BulkheadDoor> context)
    {
        BulkheadDoor door = context.Target;

        if (lockAcquired)
        {
            door.OnHandClick(context.GuiHand);

            bool isFacingDoor = door.GetSide();
            bool isDoorOpened = !door.opened;

            LocalPlayer player = Resolve<LocalPlayer>();

            if (player.PlayerId.HasValue)
            {
                ushort playerId = player.PlayerId.Value;
                Resolve<IPacketSender>().Send(new BulkheadDoorStateChanged(id, playerId, isDoorOpened, isFacingDoor));
            }
        }
        else
        {
            door.gameObject.AddComponent<DenyOwnershipHand>();
            door.isValidHandTarget = false;
        }
    }

}

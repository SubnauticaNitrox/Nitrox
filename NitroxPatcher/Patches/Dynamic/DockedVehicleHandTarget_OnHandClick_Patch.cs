using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class DockedVehicleHandTarget_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((DockedVehicleHandTarget t) => t.OnHandClick(default(GUIHand)));

    private static bool skipPrefix;

    public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
    {
        Vehicle vehicle = __instance.dockingBay.GetDockedVehicle();

        if (skipPrefix || !vehicle.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"Already have an exclusive lock on this vehicle: {id}");
            return true;
        }

        HandInteraction<DockedVehicleHandTarget> context = new(__instance, hand);
        LockRequest<HandInteraction<DockedVehicleHandTarget>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAcquired, HandInteraction<DockedVehicleHandTarget> context)
    {
        if (lockAcquired)
        {
            VehicleDockingBay dockingBay = context.Target.dockingBay;
            Resolve<Vehicles>().BroadcastVehicleUndocking(dockingBay, dockingBay.GetDockedVehicle(), true);

            skipPrefix = true;
            context.Target.OnHandClick(context.GuiHand);
            skipPrefix = false;
        }
        else
        {
            //TODO: Check if this should be Hand
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Another player is using this vehicle!", false, GameInput.Button.None);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
            context.Target.isValidHandTarget = false;
        }
    }
}

using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Simulation;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using static VFXParticlesPool;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class DockedVehicleHandTarget_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((DockedVehicleHandTarget t) => t.OnHandClick(default(GUIHand)));

    private static bool skipPrefix;

    public static bool Prefix(DockedVehicleHandTarget __instance, GUIHand hand)
    {
        Vehicle vehicle = __instance.dockingBay.GetDockedVehicle();

        if (skipPrefix || !vehicle.TryGetIdOrWarn(out NitroxId vehicleId))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(vehicleId))
        {
            Log.Debug($"Already have an exclusive lock on this vehicle: {vehicleId}");
            return true;
        }

        HandInteraction<DockedVehicleHandTarget> context = new(__instance, hand);
        LockRequest<HandInteraction<DockedVehicleHandTarget>> lockRequest = new(vehicleId, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);
        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId vehicleId, bool lockAcquired, HandInteraction<DockedVehicleHandTarget> context)
    {
        if (lockAcquired)
        {
            VehicleDockingBay dockingBay = context.Target.dockingBay;
            Vehicle vehicle =  dockingBay.GetDockedVehicle();

            if (!dockingBay.TryGetIdOrWarn(out NitroxId dockId))
            {
                return;
            }

            Vehicles.EngagePlayerMovementSuppressor(vehicle);
            Resolve<IPacketSender>().Send(new VehicleUndocking(vehicleId, dockId, Resolve<IMultiplayerSession>().Reservation.PlayerId, true));

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

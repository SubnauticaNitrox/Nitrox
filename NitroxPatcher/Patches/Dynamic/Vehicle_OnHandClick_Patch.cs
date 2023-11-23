using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.Simulation;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Vehicle_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Vehicle t) => t.OnHandClick(default(GUIHand)));

    private static bool skipPrefix;

    public static bool Prefix(Vehicle __instance, GUIHand hand)
    {
        if (skipPrefix)
        {
            return true;
        }

        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            return true;
        }

        if (Resolve<SimulationOwnership>().HasExclusiveLock(id))
        {
            Log.Debug($"Already have an exclusive lock on the vehicle: {id}");
            return true;
        }

        HandInteraction<Vehicle> context = new(__instance, hand);
        LockRequest<HandInteraction<Vehicle>> lockRequest = new(id, SimulationLockType.EXCLUSIVE, ReceivedSimulationLockResponse, context);

        Resolve<SimulationOwnership>().RequestSimulationLock(lockRequest);

        return false;
    }

    private static void ReceivedSimulationLockResponse(NitroxId id, bool lockAquired, HandInteraction<Vehicle> context)
    {
        Vehicle vehicle = context.Target;

        if (lockAquired)
        {
            skipPrefix = true;
            vehicle.OnHandClick(context.GuiHand);
            skipPrefix = false;
        }
        else
        {
            vehicle.gameObject.AddComponent<DenyOwnershipHand>();
            vehicle.isValidHandTarget = false;
        }
    }
}

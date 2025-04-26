using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the emptying of a planter if local player is simulating it.
/// </summary>
public sealed partial class Planter_ResetStorage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Planter t) => t.ResetStorage());

    public static void Prefix(Planter __instance)
    {
        // We don't care if it's already empty
        if (__instance.storageContainer.container.count == 0)
        {
            return;
        }

        // This is called from WaterPark.Update so we have no proper way of suppressing it.
        // Thus we restrict to simulation ownership
        if (!__instance.TryGetIdOrWarn(out NitroxId planterId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(planterId))
        {
            return;
        }

        Resolve<IPacketSender>().Send(new ClearPlanter(planterId));
    }
}

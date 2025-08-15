using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Helper;
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
        if (__instance.TryGetIdOrWarn(out NitroxId planterId) &&
            TryGetOwnerNitroxId(__instance, out NitroxId ownerNitroxId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(ownerNitroxId))
        {
            Resolve<IPacketSender>().Send(new ClearPlanter(planterId));
        }
    }

    /// <summary>
    /// Returns <c>true</c> with <paramref name="ownerNitroxId"/> being the NitroxId of the entity responsible for the planter if it can be found.
    /// Else returns <c>false</c>.
    /// </summary>
    public static bool TryGetOwnerNitroxId(Planter planter, out NitroxId ownerNitroxId)
    {
        if (!planter)
        {
            Log.WarnOnce("Tried getting owner NitroxId of null planter");
            ownerNitroxId = null;
            return false;
        }

        // Multiple cases:
        // 1. outdoor planter, it is responsible for itself
        if (!planter.isIndoor)
        {
            return planter.TryGetNitroxId(out ownerNitroxId);
        }

        switch (planter.environment)
        {
            // 2. indoor planter, not in waterpark, the base is responsible
            case Planter.PlantEnvironment.Air:
                if (planter.TryGetComponentInParent(out Base parentBase))
                {
                    return parentBase.TryGetNitroxId(out ownerNitroxId);
                }
                break;

            // 3. indoor planter, in waterpark, the water park is responsible
            case Planter.PlantEnvironment.Water:
                if (planter.TryGetComponentInParent(out WaterPark waterPark))
                {
                    return waterPark.TryGetNitroxId(out ownerNitroxId);
                }
                break;
        }

        ownerNitroxId = null;
        return false;
    }
}

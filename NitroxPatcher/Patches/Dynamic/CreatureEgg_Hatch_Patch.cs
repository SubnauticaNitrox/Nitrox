using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs egg deletion when hatching.
/// </summary>
public sealed partial class CreatureEgg_Hatch_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CreatureEgg t) => t.Hatch());

    public static void Prefix(CreatureEgg __instance)
    {
        // This case always destroys the creature egg (see original code)
        if (!__instance.TryGetComponent(out WaterParkItem waterParkItem))
        {
            return;
        }

        // We don't manage eggs with no id here
        if (!__instance.TryGetNitroxId(out NitroxId eggId))
        {
            return;
        }
        
        // It is VERY IMPORTANT to check for simulation ownership on the water park and not on the egg
        // since that's the convention we chose
        if (waterParkItem.currentWaterPark.TryGetNitroxId(out NitroxId waterParkId) &&
            Resolve<SimulationOwnership>().HasAnyLockType(waterParkId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(eggId));
        }
    }
}

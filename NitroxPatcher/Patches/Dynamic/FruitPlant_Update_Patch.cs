using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts fruit respawns only by the simulating player.
/// </summary>
public sealed partial class FruitPlant_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((FruitPlant t) => t.Update());

    public static bool Prefix(FruitPlant __instance, out (float, NitroxId, Plantable) __state)
    {
        // To avoid too many iterations of the code below, we check the conditions of the Update's while loop
        if (DayNightCycle.main.timePassedAsFloat < __instance.timeNextFruit || __instance.inactiveFruits.Count == 0)
        {
            __state = (__instance.timeNextFruit, null, null);
            return false;
        }

        // If the NitroxEntity is on the same object (e.g. Kelp)
        if (__instance.TryGetNitroxId(out NitroxId entityId))
        {
            __state = (__instance.timeNextFruit, entityId, null);
            return Resolve<SimulationOwnership>().HasAnyLockType(entityId);
        }

        // If the NitroxEntity is on the distant Plantable object (e.g. fruit tree in a plant pot)
        if (PickPrefab_SetPickedUp_Patch.TryGetPlantable(__instance, out Plantable plantable) &&
            plantable.TryGetNitroxId(out entityId) &&
            plantable.currentPlanter && plantable.currentPlanter.TryGetNitroxId(out NitroxId planterId))
        {
            __state = (__instance.timeNextFruit, entityId, plantable);
            // In this precise case, we look for ownership over the planter and not the plant
            // This simplifies a lot simulation ownership distribution
            return Resolve<SimulationOwnership>().HasAnyLockType(planterId);
        }

        __state = (__instance.timeNextFruit, null, null);
        return true;
    }

    public static void Postfix(FruitPlant __instance, (float, NitroxId, Plantable) __state)
    {
        // If no change was made
        if (__state.Item1 == __instance.timeNextFruit)
        {
            return;
        }

        // If the NitroxEntity is on the same object
        if (!__state.Item3)
        {
            FruitPlantMetadata metadata = Resolve<FruitPlantMetadataExtractor>().Extract(__instance);
            Resolve<IPacketSender>().Send(new EntityMetadataUpdate(__state.Item2, metadata));
        }
        // If the NitroxEntity is on a distant Plantable object
        else
        {
            // TODO: Refer to the TODO in PlantableMetadata.
            // When TODO is done, change this to only update the FruitPlant metadata (like the above if)
            PlantableMetadata metadata = Resolve<PlantableMetadataExtractor>().Extract(__state.Item3);
            Resolve<IPacketSender>().Send(new EntityMetadataUpdate(__state.Item2, metadata));
        }
    }
}

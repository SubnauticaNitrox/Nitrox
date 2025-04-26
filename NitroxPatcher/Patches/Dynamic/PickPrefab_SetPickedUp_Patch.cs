using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.Helpers;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts fruit harvesting (metadata update) when <see cref="PickPrefab"/> is under a <see cref="FruitPlant"/>.
/// </summary>
public sealed partial class PickPrefab_SetPickedUp_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PickPrefab t) => t.SetPickedUp());

    public static void Postfix(PickPrefab __instance)
    {
        if (!__instance.TryGetReference(out FruitPlant fruitPlant))
        {
            return;
        }

        // This broadcast doesn't require to be simulating the plant because harvesting a fruit is a local action
        // therefore it needs to be known by other players

        // In case the FruitPlant is directly on the entity object (which has an id, just like kelp)
        if (fruitPlant.TryGetNitroxId(out NitroxId fruitPlantId))
        {
            FruitPlantMetadata metadata = Resolve<FruitPlantMetadataExtractor>().Extract(fruitPlant);
            Resolve<IPacketSender>().Send(new EntityMetadataUpdate(fruitPlantId, metadata));
        }
        // In case the FruitPlant is on the GrownPlant object (doesn't have the id on it)
        else if (TryGetPlantable(fruitPlant, out Plantable plantable) && plantable.TryGetNitroxId(out NitroxId plantableId))
        {
            // TODO: Refer to the TODO in PlantableMetadata.
            // When TODO is done, change this to only update the FruitPlant metadata (like the above if)
            PlantableMetadata metadata = Resolve<PlantableMetadataExtractor>().Extract(plantable);
            Resolve<IPacketSender>().Send(new EntityMetadataUpdate(plantableId, metadata));
        }
    }

    public static bool TryGetPlantable(FruitPlant fruitPlant, out Plantable plantable)
    {
        if (fruitPlant.TryGetComponent(out GrownPlant grownPlant) && grownPlant.seed)
        {
            plantable = grownPlant.seed;
            return true;
        }

        plantable = null;
        return false;
    }
}

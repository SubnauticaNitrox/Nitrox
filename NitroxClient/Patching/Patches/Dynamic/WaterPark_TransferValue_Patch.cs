using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Services.Multiplayer;
using static NitroxClient.Services.Multiplayer.BuildingService;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
///     Transfers the NitroxEntity to the new main module when two WaterParks are merged.
/// </summary>
internal sealed partial class WaterPark_TransferValue_Patch : NitroxPatch, IDynamicPatch
{
    private static BuildingService buildingService;
    private static EntityMetadataManager entityMetadataManager;
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => WaterPark.TransferValue(default, default));

    private static TemporaryBuildData Temp => buildingService.Temp;

    public WaterPark_TransferValue_Patch(BuildingService bs, EntityMetadataManager emm)
    {
        buildingService = bs ?? throw new ArgumentNullException(nameof(bs));
        entityMetadataManager = emm ?? throw new ArgumentNullException(nameof(emm));
    }

    public static void Prefix(WaterPark srcWaterPark, WaterPark dstWaterPark)
    {
        if (!srcWaterPark.TryGetNitroxId(out NitroxId sourceId))
        {
            return;
        }

        // Happens when you regroup a bottom waterpark and an upper waterpark by a middle waterpark
        // The waterpark pieces are merged into the bottom one
        if (dstWaterPark.TryGetNitroxId(out NitroxId destinationId))
        {
            Log.Debug($"Changed id when transferring value, from {sourceId} to {destinationId}");
            Temp.ChildrenTransfer = (sourceId, destinationId);
            return;
        }

        // Happens when you destroy the bottom piece of a waterpark higher than 1
        if (dstWaterPark.height == 0)
        {
            NitroxId newId = Temp.NewWaterPark?.Id ?? new();
            Log.Debug($"Changed id when transferring value, from nothing to {newId} [source: {sourceId}]");
            NitroxEntity.SetNewId(dstWaterPark.gameObject, newId);
            BuildingPostSpawner.SetupWaterPark(dstWaterPark, newId);
            if (Temp.NewWaterPark == null)
            {
                Temp.NewWaterPark = InteriorPieceEntitySpawner.From(dstWaterPark, entityMetadataManager);
                Temp.Transfer = true;
            }
            return;
        }

        // Happens when you place a piece at the bottom of a waterpark
        // We simply take the existing water park entity to avoid unnecessary actions
        // its BaseFace will be updated with updatedChildren field in UpdateBase packet
        NitroxEntity.SetNewId(dstWaterPark.gameObject, sourceId);
        BuildingPostSpawner.SetupWaterPark(dstWaterPark, sourceId);

        // This is a little "cheat" on our own system because it means that we're cleaning any planter entity
        // because plants aren't transferred during this operation so we need the server to understand it
        // see BuildingManager.UpdateBase how ChildrenTransfer works to understand better
        Temp.ChildrenTransfer = (sourceId, sourceId);
    }
}

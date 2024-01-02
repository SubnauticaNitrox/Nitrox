using System.Reflection;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Spawning.Bases;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using static NitroxClient.GameLogic.Bases.BuildingHandler;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Transfers the NitroxEntity to the new main module when two WaterParks are merged.
/// </summary>
public sealed partial class WaterPark_TransferValue_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => WaterPark.TransferValue(default, default));

    private static TemporaryBuildData Temp => BuildingHandler.Main.Temp;

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
            if (Temp.NewWaterPark == null)
            {
                Temp.NewWaterPark = InteriorPieceEntitySpawner.From(dstWaterPark, Resolve<EntityMetadataManager>());
                Temp.Transfer = true;
            }
            return;
        }
        // Happens when you place a piece at the bottom of a waterpark
        // We simply take the existing water park entity to avoid unnecessary actions
        // its BaseFace will be updated with updatedChildren field in UpdateBase packet
        NitroxEntity.SetNewId(dstWaterPark.gameObject, sourceId);
    }
}

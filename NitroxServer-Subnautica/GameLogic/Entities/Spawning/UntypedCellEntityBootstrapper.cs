using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;

public class UntypedCellEntityBootstrapper : IEntityBootstrapper
{
    /// <summary>
    ///     In the server, cell visibility logic isn't perfectly aligned with logic in the central game's
    ///     ClipMapManager.  Some entities have long-range visibility but are encapsulated in cells with
    ///     short-range visibility.  This is reconciled in-game with custom logic for processing cell
    ///     roots.  We are looking to expand our own handling of cell entities by tracking the player
    ///     position; however, as a stop gap we force cell root level to their highest child.
    /// </summary>
    private readonly string cellRootClassId = "55d7ab35-de97-4d95-af6c-ac8d03bb54ca";

    public void Prepare(Entity entity, Entity parentEntity, DeterministicGenerator deterministicBatchGenerator)
    {
        bool hasCellRootAsParent = parentEntity != null && parentEntity.ClassId == cellRootClassId;

        if (hasCellRootAsParent && parentEntity.Level < entity.Level)
        {
            // bump the cell root up to the child's visibility level
            parentEntity.Level = entity.Level;
        }
    }
}

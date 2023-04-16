using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class InteriorPieceEntitySpawner : EntitySpawner<InteriorPieceEntity>
{
    private readonly Entities entities;

    public InteriorPieceEntitySpawner(Entities entities)
    {
        this.entities = entities;
    }

    public override IEnumerator SpawnAsync(InteriorPieceEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (entity.ParentId == null || !NitroxEntity.TryGetComponentFrom(entity.ParentId, out Base @base))
        {
            Log.Error($"Couldn't find a Base component on the parent object of InteriorPieceEntity {entity.Id}");
            yield break;
        }
        yield return NitroxBuild.RestoreInteriorPiece(entity, @base, result);
        foreach (InventoryItemEntity childItemEntity in entity.ChildEntities.OfType<InventoryItemEntity>())
        {
            Log.Debug($"Spawning child item entity: {childItemEntity}");
            yield return entities.SpawnAsync(childItemEntity);
        }
        foreach (InstalledModuleEntity childModuleEntity in entity.ChildEntities.OfType<InstalledModuleEntity>())
        {
            Log.Debug($"Spawning child module entity: {childModuleEntity}");
            yield return entities.SpawnAsync(childModuleEntity);
        }
    }

    public override bool SpawnsOwnChildren(InteriorPieceEntity entity) => true;
}

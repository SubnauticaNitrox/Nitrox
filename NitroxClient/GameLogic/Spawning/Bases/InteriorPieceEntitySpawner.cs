using NitroxClient.GameLogic.Bases.New;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using System.Collections;
using System.Collections.Generic;
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
        if (!result.Get().HasValue)
        {
            Log.Error($"Restoring interior piece failed: {entity}");
            yield break;
        }
        bool isWaterPark = entity.IsWaterPark;

        List<Entity> batch = new();
        foreach (Entity childEntity in entity.ChildEntities)
        {
            switch(childEntity)
            {
                case InventoryItemEntity:
                case InstalledModuleEntity:
                    Log.Debug($"Spawning child entity: {childEntity}");
                    batch.Add(childEntity);
                    break;

                case PlanterEntity:
                    foreach (InventoryItemEntity childItemEntity in childEntity.ChildEntities.OfType<InventoryItemEntity>())
                    {
                        Log.Debug($"Spawning planter child item entity: {childItemEntity}");
                        batch.Add(childItemEntity);
                    }
                    break;

                case WorldEntity:
                    if (isWaterPark)
                    {
                        batch.Add(childEntity);
                    }
                    break;
            }
        }

        yield return entities.SpawnBatchAsync(batch, true);

        if (isWaterPark)
        {
            foreach (Planter planter in result.Get().Value.GetComponentsInChildren<Planter>(true))
            {
                yield return planter.DeserializeAsync();
            }
        }

        if (result.Get().Value.TryGetComponent(out PowerSource powerSource))
        {
            NitroxBuild.SetupPower(powerSource);
        }
    }

    public override bool SpawnsOwnChildren(InteriorPieceEntity entity) => true;
}

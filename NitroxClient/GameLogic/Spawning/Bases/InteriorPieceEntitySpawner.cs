using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Bases;

public class InteriorPieceEntitySpawner : EntitySpawner<InteriorPieceEntity>
{
    private readonly Entities entities;
    private readonly EntityMetadataManager entityMetadataManager;

    public InteriorPieceEntitySpawner(Entities entities, EntityMetadataManager entityMetadataManager)
    {
        this.entities = entities;
        this.entityMetadataManager = entityMetadataManager;
    }

    protected override IEnumerator SpawnAsync(InteriorPieceEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (entity.ParentId == null || !NitroxEntity.TryGetComponentFrom(entity.ParentId, out Base @base))
        {
            Log.Error($"Couldn't find a Base component on the parent object of InteriorPieceEntity {entity.Id}");
            yield break;
        }
        yield return RestoreInteriorPiece(entity, @base, result);
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
                    batch.Add(childEntity);
                    break;

                case PlanterEntity:
                    foreach (InventoryItemEntity childItemEntity in childEntity.ChildEntities.OfType<InventoryItemEntity>())
                    {
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

        if (isWaterPark)
        {
            // Must happen before child plant spawning
            foreach (Planter planter in result.Get().Value.GetComponentsInChildren<Planter>(true))
            {
                yield return planter.DeserializeAsync();
            }
        }

        yield return entities.SpawnBatchAsync(batch, true);

        if (result.Get().Value.TryGetComponent(out PowerSource powerSource))
        {
            // TODO: Have synced/restored power
            powerSource.SetPower(powerSource.maxPower);
        }
    }

    protected override bool SpawnsOwnChildren(InteriorPieceEntity entity) => true;

    public IEnumerator RestoreInteriorPiece(InteriorPieceEntity interiorPiece, Base @base, TaskResult<Optional<GameObject>> result = null)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, classId: interiorPiece.ClassId))
        {
            TaskResult<GameObject> prefabResult = new();
            yield return DefaultWorldEntitySpawner.RequestPrefab(interiorPiece.ClassId, prefabResult);
            if (!prefabResult.Get())
            {
                Log.Error($"Couldn't find a prefab for interior piece of ClassId {interiorPiece.ClassId}");
                yield break;
            }
            prefab = prefabResult.Get();
        }

        Base.Face face = interiorPiece.BaseFace.ToUnity();
        face.cell += @base.GetAnchor();
        GameObject moduleObject = @base.SpawnModule(prefab, face);
        if (moduleObject)
        {
            NitroxEntity.SetNewId(moduleObject, interiorPiece.Id);
            yield return BuildingPostSpawner.ApplyPostSpawner(moduleObject, interiorPiece.Id);
            entityMetadataManager.ApplyMetadata(moduleObject, interiorPiece.Metadata);
            result.Set(moduleObject);
        }
    }

    public static InteriorPieceEntity From(IBaseModule module, EntityMetadataManager entityMetadataManager)
    {
        InteriorPieceEntity interiorPiece = InteriorPieceEntity.MakeEmpty();
        GameObject gameObject = (module as Component).gameObject;
        if (gameObject && gameObject.TryGetComponent(out PrefabIdentifier identifier))
        {
            interiorPiece.ClassId = identifier.ClassId;
        }
        else
        {
            Log.Warn($"Couldn't find an identifier for the interior piece {module.GetType()}");
        }

        if (gameObject.TryGetIdOrWarn(out NitroxId entityId))
        {
            interiorPiece.Id = entityId;
        }

        if (gameObject.TryGetComponentInParent(out Base parentBase, true) &&
            parentBase.TryGetNitroxId(out NitroxId parentId))
        {
            interiorPiece.ParentId = parentId;
        }

        switch (module)
        {
            case LargeRoomWaterPark:
                PlanterEntity leftPlanter = new(interiorPiece.Id.Increment(), interiorPiece.Id);
                PlanterEntity rightPlanter = new(leftPlanter.Id.Increment(), interiorPiece.Id);
                interiorPiece.ChildEntities.Add(leftPlanter);
                interiorPiece.ChildEntities.Add(rightPlanter);
                break;
            // When you deconstruct (not entirely) then construct back those pieces, they keep their inventories
            case BaseNuclearReactor baseNuclearReactor:
                interiorPiece.ChildEntities.AddRange(Items.GetEquipmentModuleEntities(baseNuclearReactor.equipment, entityId, entityMetadataManager));
                break;
            case BaseBioReactor baseBioReactor:
                foreach (ItemsContainer.ItemGroup itemGroup in baseBioReactor.container._items.Values)
                {
                    foreach (InventoryItem item in itemGroup.items)
                    {
                        interiorPiece.ChildEntities.Add(Items.ConvertToInventoryItemEntity(item.item.gameObject, entityMetadataManager));
                    }
                }
                break;
        }

        interiorPiece.BaseFace = module.moduleFace.ToDto();

        return interiorPiece;
    }

    public static IEnumerator RestoreMapRoom(Base @base, MapRoomEntity mapRoomEntity)
    {
        MapRoomFunctionality mapRoomFunctionality = @base.GetMapRoomFunctionalityForCell(mapRoomEntity.Cell.ToUnity());
        if (!mapRoomFunctionality)
        {
            Log.Error($"Couldn't find MapRoomFunctionality in base for cell {mapRoomEntity.Cell}");
            yield break;
        }
        NitroxEntity.SetNewId(mapRoomFunctionality.gameObject, mapRoomEntity.Id);
    }
}

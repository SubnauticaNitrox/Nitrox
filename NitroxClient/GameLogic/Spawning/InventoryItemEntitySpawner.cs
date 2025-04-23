using System;
using System.Collections;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Spawning;

public class InventoryItemEntitySpawner(EntityMetadataManager entityMetadataManager) : SyncEntitySpawner<InventoryItemEntity>
{
    private readonly EntityMetadataManager entityMetadataManager = entityMetadataManager;

    protected override IEnumerator SpawnAsync(InventoryItemEntity entity, TaskResult<Optional<GameObject>> result)
    {        
        if (!CanSpawn(entity, out GameObject parentObject, out ItemsContainer container, out string errorLog))
        {
            Log.Info(errorLog);
            result.Set(Optional.Empty);
            yield break;
        }

        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, entity.Id, gameObjectResult);
        GameObject gameObject = gameObjectResult.Get();

        SetupObject(entity, gameObject, parentObject, container);

        result.Set(Optional.Of(gameObject));
    }

    protected override bool SpawnSync(InventoryItemEntity entity, TaskResult<Optional<GameObject>> result)
    {
        if (!DefaultWorldEntitySpawner.TryGetCachedPrefab(out GameObject prefab, entity.TechType.ToUnity(), entity.ClassId))
        {
            return false;
        }
        if (!CanSpawn(entity, out GameObject parentObject, out ItemsContainer container, out string errorLog))
        {
            Log.Error(errorLog);
            return true;
        }

        GameObject gameObject = GameObjectHelper.SpawnFromPrefab(prefab, entity.Id);

        SetupObject(entity, gameObject, parentObject, container);

        result.Set(gameObject);
        return true;
    }

    protected override bool SpawnsOwnChildren(InventoryItemEntity entity) => false;

    private bool CanSpawn(InventoryItemEntity entity, out GameObject parentObject, out ItemsContainer container, out string errorLog)
    {
        Optional<GameObject> owner = NitroxEntity.GetObjectFrom(entity.ParentId);
        if (!owner.HasValue)
        {
            parentObject = null;
            container = null;
            errorLog = $"Unable to find inventory container with id {entity.Id} for {entity}";
            return false;
        }

        Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(owner.Value);

        if (!opContainer.HasValue)
        {
            parentObject = null;
            container = null;
            errorLog = $"Could not find container field on GameObject {parentObject.AliveOrNull()?.GetFullHierarchyPath()}";
            return false;
        }

        parentObject = owner.Value;
        container = opContainer.Value;
        errorLog = null;
        return true;
    }

    private void SetupObject(InventoryItemEntity entity, GameObject gameObject, GameObject parentObject, ItemsContainer container)
    {
        Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
        pickupable.Initialize();

        InventoryItem inventoryItem = new(pickupable);

        // Items eventually get "secured" once a player gets into a SubRoot (or for other reasons) so we need to force this state by default
        // so that player don't risk their whole inventory if they reconnect in the water.
        pickupable.destroyOnDeath = false;

        bool isPlanter = parentObject.TryGetComponent(out Planter planter);
        bool subscribedValue = false;
        if (isPlanter)
        {
            subscribedValue = planter.subscribed;
            planter.Subscribe(false);
        }

        using (PacketSuppressor<EntityReparented>.Suppress())
        using (PacketSuppressor<PlayerQuickSlotsBindingChanged>.Suppress())
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            container.UnsafeAdd(inventoryItem);
            Log.Debug($"Received: Added item {pickupable.GetTechType()} ({entity.Id}) to container {parentObject.GetFullHierarchyPath()}");
        }

        if (isPlanter)
        {
            planter.Subscribe(subscribedValue);

            if (entity.Metadata is PlantableMetadata metadata)
            {
                PostponeAddNotification(() => planter.subscribed, () => planter, true, () =>
                {
                    // Adapted from Planter.AddItem(InventoryItem) to be able to call directly AddItem(Plantable, slotID) with our parameters
                    Plantable plantable = pickupable.GetComponent<Plantable>();
                    pickupable.SetTechTypeOverride(plantable.plantTechType, false);
                    inventoryItem.isEnabled = false;
                    planter.AddItem(plantable, metadata.SlotID);

                    // Apply the plantable metadata after the GrowingPlant (or the GrownPlant) is spawned
                    // this will allow the GrowingPlant to know about its progress
                    entityMetadataManager.ApplyMetadata(plantable.gameObject, metadata);

                    // Plant spawning occurs in multiple steps over frames:
                    // spawning the item, adding it to the planter, having the GrowingPlant created, and eventually having it create a GrownPlant (when progress == 1)
                    // therefore we give the metadata to the object so it can be used when required
                    if (metadata.FruitPlantMetadata != null && plantable.growingPlant && plantable.growingPlant.GetProgress() == 1f)
                    {
                        plantable.growingPlant.AddReference(metadata.FruitPlantMetadata);
                    }

                    // NB: Entities.SpawnBatchAsync (which is the function calling the current spawner)
                    // will still apply the metadata another time but we don't care as it's not destructive
                });
            }
        }
        else if (parentObject.TryGetComponent(out Trashcan trashcan))
        {
            PostponeAddNotification(() => trashcan.subscribed, () => trashcan, false, () =>
            {
                trashcan.AddItem(inventoryItem);
            });
        }
    }

    private static void PostponeAddNotification(Func<bool> subscribed, Func<bool> instanceValid, bool callbackIfAlreadySubscribed, Action callback)
    {
        IEnumerator PostponedAddCallback()
        {
            yield return new WaitUntil(() => subscribed() || !instanceValid());
            if (instanceValid())
            {
                using (PacketSuppressor<EntityReparented>.Suppress())
                using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
                {
                    callback();
                }
            }
        }

        if (!subscribed())
        {
            CoroutineHost.StartCoroutine(PostponedAddCallback());
        }
        else if (callbackIfAlreadySubscribed)
        {
            callback();
        }
    }
}

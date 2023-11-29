using System.Collections;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning;

public class InventoryItemEntitySpawner : SyncEntitySpawner<InventoryItemEntity>
{
    protected override IEnumerator SpawnAsync(InventoryItemEntity entity, TaskResult<Optional<GameObject>> result)
    {        
        if (!CanSpawn(entity, out GameObject parentObject, out ItemsContainer container, out string errorLog))
        {
            Log.Info(errorLog);
            result.Set(Optional.Empty);
            yield break;
        }

        TaskResult<GameObject> gameObjectResult = new();
        yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, gameObjectResult);
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

        GameObject gameObject = Utils.SpawnFromPrefab(prefab, null);

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
        NitroxEntity.SetNewId(gameObject, entity.Id);

        Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
        pickupable.Initialize();

        using (PacketSuppressor<EntityReparented>.Suppress())
        using (PacketSuppressor<PlayerQuickSlotsBindingChanged>.Suppress())
        {
            container.UnsafeAdd(new InventoryItem(pickupable));
            Log.Debug($"Received: Added item {pickupable.GetTechType()} ({entity.Id}) to container {parentObject.GetFullHierarchyPath()}");
        }
    }
}

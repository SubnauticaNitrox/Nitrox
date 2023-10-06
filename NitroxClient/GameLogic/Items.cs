using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class Items
{
    private readonly IPacketSender packetSender;
    private readonly IMap map;
    private readonly Entities entities;

    public Items(IPacketSender packetSender, IMap map, Entities entities)
    {
        this.packetSender = packetSender;
        this.map = map;
        this.entities = entities;
    }

    public void UpdatePosition(NitroxId id, Vector3 location, Quaternion rotation)
    {
        ItemPosition itemPosition = new ItemPosition(id, location.ToDto(), rotation.ToDto());
        packetSender.Send(itemPosition);
    }

    public void PickedUp(GameObject gameObject, TechType techType)
    {
        // We want to remove any remote tracking immediately on pickup as it can cause weird behavior like holding a ghost item still in the world.
        RemoveAnyRemoteControl(gameObject);

        if (!gameObject.TryGetNitroxId(out NitroxId id))
        {
            Log.Debug($"Found item with ({gameObject.name}) with no id, assigning a new one");
            id = NitroxEntity.GenerateNewId(gameObject);
        }

        EntityPositionBroadcaster.StopWatchingEntity(id);

        InventoryItemEntity inventoryItemEntity = ConvertToInventoryItemEntity(gameObject);

        // Some picked up entities are not known by the server for several reasons.  First it can be picked up via a spawn item command.  Another
        // example is that some obects are not 'real' objects until they are clicked and end up spawning a prefab.  For example, the fire extinguisher
        // in the escape pod (mono: IntroFireExtinguisherHandTarget) or Creepvine seeds (mono: PickupPrefab).  When clicked, these spawn new prefabs
        // directly into the player's inventory.  These will ultimately be registered server side with the above inventoryItemEntity.
        entities.MarkAsSpawned(inventoryItemEntity);

        Log.Debug($"PickedUp {id} {techType}");

        PickupItem pickupItem = new(id, inventoryItemEntity);
        packetSender.Send(pickupItem);
    }

    /// <summary>
    /// Tracks the object (as dropped) and notifies the server to spawn the item for other players.
    /// </summary>
    public void Dropped(GameObject gameObject, TechType? techType = null)
    {
        techType ??= CraftData.GetTechType(gameObject);
        // there is a theoretical possibility of a stray remote tracking packet that re-adds the monobehavior, this is purely a safety call.
        RemoveAnyRemoteControl(gameObject);

        NitroxId id = NitroxEntity.GetIdOrGenerateNew(gameObject);
        Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(gameObject);
        bool inGlobalRoot = map.GlobalRootTechTypes.Contains(techType.Value.ToDto());
        string classId = gameObject.GetComponent<PrefabIdentifier>().ClassId;

        WorldEntity droppedItem = new(gameObject.transform.ToWorldDto(), 0, classId, inGlobalRoot, id, techType.Value.ToDto(), metadata.OrNull(), null, new List<Entity>())
        {
            ChildEntities = GetPrefabChildren(gameObject, id).ToList()
        };

        // There are two specific cases which we need to notice:
        // 1. If the item is dropped in a WaterPark
        if (gameObject.GetComponent<Pickupable>() && TryGetCurrentWaterParkId(out NitroxId waterParkId))
        {
            droppedItem.ParentId = waterParkId;
            // We cast it to an entity type that is always seeable by clients
            // therefore, the packet will be redirected to everyone
            droppedItem = GlobalRootEntity.From(droppedItem);
        }
        else
        {
            // 2. You can't drop items in bases but you can place small objects like figures and posters which are put right under the base object
            // NB: They are recognizable by their PlaceTool from which the Place() function executes the current code
            SubRoot currentSub = Player.main.GetCurrentSub();
            if (currentSub && currentSub.TryGetNitroxId(out NitroxId parentId))
            {
                droppedItem.ParentId = parentId;
            }
        }
        Log.Debug($"Dropping item: {droppedItem}");

        packetSender.Send(new EntitySpawnedByClient(droppedItem));
    }

    public void Created(GameObject gameObject)
    {
        InventoryItemEntity inventoryItemEntity = ConvertToInventoryItemEntity(gameObject);
        entities.MarkAsSpawned(inventoryItemEntity);

        if (packetSender.Send(new EntitySpawnedByClient(inventoryItemEntity)))
        {
            Log.Debug($"Creation of item {gameObject.name} into the player's inventory {inventoryItemEntity}");
        }
    }

    // This function will record any notable children of the dropped item as a PrefabChildEntity.  In this case, a 'notable'
    // child is one that UWE has tagged with a PrefabIdentifier (class id) and has entity metadata that can be extracted. An
    // example would be recording a Battery PrefabChild inside of a Flashlight WorldEntity.
    public static IEnumerable<Entity> GetPrefabChildren(GameObject gameObject, NitroxId parentId)
    {
        foreach (IGrouping<string, PrefabIdentifier> prefabGroup in gameObject.GetAllComponentsInChildren<PrefabIdentifier>()
                                                                              .Where(prefab => prefab.gameObject != gameObject)
                                                                              .GroupBy(prefab => prefab.classId))
        {
            int indexInGroup = 0;

            foreach (PrefabIdentifier prefab in prefabGroup)
            {
                NitroxId id = NitroxEntity.GetIdOrGenerateNew(prefab.gameObject); // We do this here bc a MetadataExtractor could be requiring the id to increment or so
                Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(prefab.gameObject);

                if (metadata.HasValue)
                {
                    TechTag techTag = prefab.gameObject.GetComponent<TechTag>();
                    TechType techType = (techTag) ? techTag.type : TechType.None;

                    yield return new PrefabChildEntity(id, prefab.classId, techType.ToDto(), indexInGroup, metadata.Value, parentId);

                    indexInGroup++;
                }
            }
        }
    }

    public static InventoryItemEntity ConvertToInventoryItemEntity(GameObject gameObject)
    {
        NitroxId itemId = NitroxEntity.GetIdOrGenerateNew(gameObject); // id may not exist, create if missing
        string classId = gameObject.RequireComponent<PrefabIdentifier>().ClassId;
        TechType techType = gameObject.RequireComponent<Pickupable>().GetTechType();
        Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(gameObject);
        List<Entity> children = GetPrefabChildren(gameObject, itemId).ToList();

        // Newly created objects are always placed into the player's inventory.
        if (!Player.main.TryGetNitroxId(out NitroxId ownerId))
        {
            throw new InvalidOperationException("[Items] Player has no id! Couldn't parent InventoryItem.");
        }

        InventoryItemEntity inventoryItemEntity = new(itemId, classId, techType.ToDto(), metadata.OrNull(), ownerId, children);
        BatteryChildEntityHelper.TryPopulateInstalledBattery(gameObject, inventoryItemEntity.ChildEntities, itemId);

        return inventoryItemEntity;
    }

    /// <summary>
    /// Some items might be remotely simulated if they were dropped by other players.  We'll want to remove
    /// any remote tracking when we actively handle the item.
    /// </summary>
    private void RemoveAnyRemoteControl(GameObject gameObject)
    {
        UnityEngine.Object.Destroy(gameObject.GetComponent<RemotelyControlled>());
    }

    private bool TryGetCurrentWaterParkId(out NitroxId waterParkId)
    {
        if (Player.main && Player.main.currentWaterPark &&
            Player.main.currentWaterPark.TryGetNitroxId(out waterParkId))
        {
            return true;
        }
        waterParkId = null;
        return false;
    }

    public static List<InstalledModuleEntity> GetEquipmentModuleEntities(Equipment equipment, NitroxId equipmentId)
    {
        List<InstalledModuleEntity> entities = new();
        foreach (KeyValuePair<string, InventoryItem> itemEntry in equipment.equipment)
        {
            InventoryItem item = itemEntry.Value;
            if (item != null)
            {
                Pickupable pickupable = item.item;
                string classId = pickupable.RequireComponent<PrefabIdentifier>().ClassId;
                NitroxId itemId = NitroxEntity.GetIdOrGenerateNew(pickupable.gameObject);
                Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(pickupable.gameObject);
                List<Entity> children = GetPrefabChildren(pickupable.gameObject, itemId).ToList();

                entities.Add(new(itemEntry.Key, classId, itemId, pickupable.GetTechType().ToDto(), metadata.OrNull(), equipmentId, children));
            }
        }
        return entities;
    }
}

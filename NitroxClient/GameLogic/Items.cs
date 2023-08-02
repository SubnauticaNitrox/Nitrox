using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic.Entities;
using System.Collections.Generic;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures.GameLogic;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxClient.GameLogic.Helper;

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

        Optional<NitroxId> waterparkId = GetCurrentWaterParkId();
        NitroxId id = NitroxEntity.GetIdOrGenerateNew(gameObject);
        Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(gameObject);
        bool inGlobalRoot = map.GlobalRootTechTypes.Contains(techType.Value.ToDto());
        string classId = gameObject.GetComponent<PrefabIdentifier>().ClassId;
        WorldEntity droppedItem = new(gameObject.transform.ToWorldDto(), 0, classId, inGlobalRoot, waterparkId.OrNull(), false, id, techType.Value.ToDto(), metadata.OrNull(), null, new List<Entity>())
        {
            ChildEntities = GetPrefabChildren(gameObject, id).ToList()
        };

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

    private InventoryItemEntity ConvertToInventoryItemEntity(GameObject gameObject)
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

    private Optional<NitroxId> GetCurrentWaterParkId()
    {
        Player player = Utils.GetLocalPlayer().GetComponent<Player>();
        if (player == null)
        {
            return Optional.Empty;
        }

        WaterPark currentWaterPark = player.currentWaterPark;
        if (currentWaterPark == null)
        {
            return Optional.Empty;
        }

        return currentWaterPark.GetId();
    }
}

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

namespace NitroxClient.GameLogic
{
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

            NitroxId id = NitroxEntity.GetId(gameObject);

            EntityPositionBroadcaster.StopWatchingEntity(id);

            // Some picked up entities are not known by the server for several reasons.  First it can be picked up via a spawn item command.  Another
            // example is that some obects are not 'real' objects until they are clicked and end up spawning a prefab.  For example, the fire extinguisher
            // in the escape pod (mono: IntroFireExtinguisherHandTarget) or Creepvine seeds (mono: PickupPrefab).  When clicked, these spawn new prefabs
            // directly into the player's inventory.  In this pickup function, we can let the server know about this by sending an EntitySpawn packet; 
            // however, the disavantage with doing this in one place is that other players may not 'see' the action (such as picking creepvine fruit). 
            // This may be intended for things like the fire extinguisher because it lets all players get it.  As we sync these actions, this statement
            // will no longer be true and will no longer send a created packet. 
            if (!entities.IsKnownEntity(id))
            {
                Created(gameObject);
            }

            Vector3 itemPosition = gameObject.transform.position;

            Log.Info($"PickedUp {id} {techType}");

            PickupItem pickupItem = new PickupItem(itemPosition.ToDto(), id, techType.ToDto());
            packetSender.Send(pickupItem);
        }

        public void Dropped(GameObject gameObject, TechType techType)
        {
            // there is a theoretical possibility of a stray remote tracking packet that re-adds the monobehavior, this is purely a safety call.
            RemoveAnyRemoteControl(gameObject);

            Optional<NitroxId> waterparkId = GetCurrentWaterParkId();
            NitroxId id = NitroxEntity.GetId(gameObject);
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(gameObject);

            bool inGlobalRoot = map.GlobalRootTechTypes.Contains(techType.ToDto());
            string classId = gameObject.GetComponent<PrefabIdentifier>().ClassId;

            WorldEntity droppedItem = new WorldEntity(gameObject.transform.ToWorldDto(), 0, classId, inGlobalRoot, waterparkId.OrNull(), false, id, techType.ToDto(), metadata.OrNull(), null, new List<Entity>());
            droppedItem.ChildEntities = GetPrefabChildren(gameObject, id).ToList();

            Log.Debug($"Dropping item: {droppedItem}");

            EntitySpawnedByClient spawnedPacket = new EntitySpawnedByClient(droppedItem);
            packetSender.Send(spawnedPacket);
        }

        public void Created(GameObject gameObject)
        {
            NitroxId itemId = NitroxEntity.GetId(gameObject);
            string classId = gameObject.RequireComponent<PrefabIdentifier>().ClassId;
            TechType techType = gameObject.RequireComponent<Pickupable>().GetTechType();
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(gameObject);
            List<Entity> children = GetPrefabChildren(gameObject, itemId).ToList();

            // Newly created objects are always placed into the player's inventory.
            NitroxId ownerId = NitroxEntity.GetId(Player.main.gameObject);

            InventoryItemEntity inventoryItemEntity = new(itemId, classId, techType.ToDto(), metadata.OrNull(), ownerId, children);
            entities.MarkAsSpawned(inventoryItemEntity);

            if (packetSender.Send(new EntitySpawnedByClient(inventoryItemEntity)))
            {
                Log.Debug($"Creation of item {techType} into the player's inventory {inventoryItemEntity}");
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
                    Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(prefab.gameObject);

                    if (metadata.HasValue)
                    {
                        NitroxId id = NitroxEntity.GetId(prefab.gameObject);
                        TechTag techTag = prefab.gameObject.GetComponent<TechTag>();
                        TechType techType = (techTag) ? techTag.type : TechType.None;

                        yield return new PrefabChildEntity(id, prefab.classId, techType.ToDto(), indexInGroup, metadata.Value, parentId);

                        indexInGroup++;
                    }
                }
            }
        }

        private void RemoveAnyRemoteControl(GameObject gameObject)
        {
            // Some items might be remotely simulated if they were dropped by other players.  We'll want to remove
            // any remote tracking when we actively handle the item. 
            RemotelyControlled remotelyControlled = gameObject.GetComponent<RemotelyControlled>();
            Object.Destroy(remotelyControlled);
        }

        private Optional<NitroxId> GetCurrentWaterParkId()
        {
            Player player = Utils.GetLocalPlayer().GetComponent<Player>();

            if (player != null)
            {
                WaterPark currentWaterPark = player.currentWaterPark;

                if (currentWaterPark != null)
                {
                    NitroxId waterParkId = NitroxEntity.GetId(currentWaterPark.gameObject);
                    return Optional.Of(waterParkId);
                }
            }

            return Optional.Empty;
        }
    }
}

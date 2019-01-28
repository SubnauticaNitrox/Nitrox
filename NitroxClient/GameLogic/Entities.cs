using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;

        private readonly HashSet<string> alreadySpawnedGuids = new HashSet<string>();  //TODO: refresh on pickup
        private readonly DefaultEntitySpawner defaultEntitySpawner = new DefaultEntitySpawner();
        private readonly SerializedEntitySpawner serializedEntitySpawner = new SerializedEntitySpawner();
        private readonly Dictionary<TechType, IEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IEntitySpawner>();

        public Entities(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackEntitySpawner(defaultEntitySpawner);
        }

        public void BroadcastTransforms(Dictionary<string, GameObject> gameObjectsByGuid)
        {
            EntityTransformUpdates update = new EntityTransformUpdates();

            foreach (KeyValuePair<string, GameObject> gameObjectWithGuid in gameObjectsByGuid)
            {
                GameObject go = gameObjectWithGuid.Value;

                if (go != null)
                {
                    update.AddUpdate(gameObjectWithGuid.Key, gameObjectWithGuid.Value.transform.position, gameObjectWithGuid.Value.transform.rotation);
                }
            }

            packetSender.Send(update);
        }

        public void Spawn(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (!alreadySpawnedGuids.Contains(entity.Guid))
                {
                    Spawn(entity, Optional<GameObject>.Empty());
                }
                else
                {
                    UpdatePosition(entity);
                }
            }
        }
        
        private void Spawn(Entity entity, Optional<GameObject> parent)
        {
            alreadySpawnedGuids.Add(entity.Guid);

            IEntitySpawner entitySpawner = ResolveEntitySpawner(entity);
            Optional<GameObject> gameObject = entitySpawner.Spawn(entity, parent);

            foreach (Entity childEntity in entity.ChildEntities)
            {
                alreadySpawnedGuids.Add(childEntity.Guid);

                if (!entitySpawner.SpawnsOwnChildren())
                {
                    Spawn(childEntity, gameObject);
                }
            }

            if (gameObject.IsPresent())
            {
                gameObject.Get().AddComponent<NitroxEntity>();
            }
        }

        private IEntitySpawner ResolveEntitySpawner(Entity entity)
        {
            if (entity.SerializedGameObject != null)
            {
                return serializedEntitySpawner;
            }

            if (customSpawnersByTechType.ContainsKey(entity.TechType))
            {
                return customSpawnersByTechType[entity.TechType];
            }

            return defaultEntitySpawner;
        }

        private void UpdatePosition(Entity entity)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(entity.Guid);

            if (opGameObject.IsPresent())
            {
                opGameObject.Get().transform.position = entity.Position;
                opGameObject.Get().transform.localRotation = entity.Rotation;
                opGameObject.Get().transform.localScale = entity.Scale;
            }
            else
            {
                Log.Error("Entity was already spawned but not found(is it in another chunk?) guid: " + entity.Guid + " " + entity.TechType + " " + entity.ClassId);
            }
        }

        public bool WasSpawnedByServer(string guid)
        {
            return alreadySpawnedGuids.Contains(guid);
        }

    }
}

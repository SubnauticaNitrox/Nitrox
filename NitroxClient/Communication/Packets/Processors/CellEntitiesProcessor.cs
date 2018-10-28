using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class CellEntitiesProcessor : ClientPacketProcessor<CellEntities>
    {
        private readonly IPacketSender packetSender;
        private readonly INitroxLogger log;
        private readonly HashSet<string> alreadySpawnedGuids = new HashSet<string>();
        private readonly DefaultEntitySpawner defaultEntitySpawner = new DefaultEntitySpawner();
        private readonly Dictionary<TechType, IEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IEntitySpawner>();

        public CellEntitiesProcessor(IPacketSender packetSender, INitroxLogger logger)
        {
            log = logger;
            this.packetSender = packetSender;
            
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackEntitySpawner(defaultEntitySpawner);
        }

        public override void Process(CellEntities packet)
        {
            foreach (Entity entity in packet.Entities)
            {
                if (!alreadySpawnedGuids.Contains(entity.Guid))
                {
                    SpawnEntity(entity, Optional<GameObject>.Empty());
                }
                else
                {
                    UpdateEntityPosition(entity);
                }
            }
        }

        private void SpawnEntity(Entity entity, Optional<GameObject> parent)
        {
            IEntitySpawner entitySpawner = ResolveEntitySpawner(entity.TechType);
            Optional<GameObject> gameObject = entitySpawner.Spawn(entity, parent);
            alreadySpawnedGuids.Add(entity.Guid);

            foreach(Entity childEntity in entity.ChildEntities)
            {
                if (!entitySpawner.SpawnsOwnChildren())
                {
                    SpawnEntity(childEntity, gameObject);
                }

                alreadySpawnedGuids.Add(childEntity.Guid);
            }
        }

        private IEntitySpawner ResolveEntitySpawner(TechType techType)
        {
            if (customSpawnersByTechType.ContainsKey(techType))
            {
                return customSpawnersByTechType[techType];
            }

            return defaultEntitySpawner;
        }

        private void UpdateEntityPosition(Entity entity)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(entity.Guid);

            if (opGameObject.IsPresent())
            {
                opGameObject.Get().transform.position = entity.Position;
                opGameObject.Get().transform.localRotation = entity.Rotation;
            }
            else
            {
                log.Error("Entity was already spawned but not found(is it in another chunk?) guid: " + entity.Guid + " " + entity.TechType + " " + entity.ClassId);
            }
        }
    }
}

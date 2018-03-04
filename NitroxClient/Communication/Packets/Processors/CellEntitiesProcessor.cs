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
        private readonly HashSet<string> alreadySpawnedGuids = new HashSet<string>();
        private readonly IEntitySpawner defaultEntitySpawner = new DefaultEntitySpawner();
        private readonly Dictionary<TechType, IEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IEntitySpawner>();

        public CellEntitiesProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            customSpawnersByTechType[TechType.None] = new NoTechTypeEntitySpawner();
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
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

            if (entity.ChildEntity.IsPresent() && gameObject.IsPresent())
            {
                SpawnEntity(entity.ChildEntity.Get(), gameObject);
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
                Log.Error("Entity was already spawned but not found(is it in another chunk?) guid: " + entity.Guid);
            }
        }
    }
}

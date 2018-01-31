using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class CellEntitiesProcessor : ClientPacketProcessor<CellEntities>
    {
        private readonly PacketSender packetSender;
        private readonly HashSet<string> alreadySpawnedGuids = new HashSet<string>();

        public CellEntitiesProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CellEntities packet)
        {
            foreach (Entity entity in packet.Entities)
            {
                if (!alreadySpawnedGuids.Contains(entity.Guid))
                {
                    SpawnEntity(entity);
                }
                else
                {
                    UpdateEntityPosition(entity);
                }
            }
        }

        private void SpawnEntity(Entity entity)
        {
            if (entity.TechType != TechType.None)
            {
                GameObject gameObject = CraftData.InstantiateFromPrefab(entity.TechType);
                gameObject.transform.position = entity.Position;
                gameObject.transform.localRotation = entity.Rotation;
                GuidHelper.SetNewGuid(gameObject, entity.Guid);
                gameObject.SetActive(true);
                LargeWorldEntity.Register(gameObject);
                CrafterLogic.NotifyCraftEnd(gameObject, entity.TechType);

                Log.Debug("Received cell entity: " + entity.Guid + " at " + entity.Position + " of type " + entity.TechType);
            }

            alreadySpawnedGuids.Add(entity.Guid);
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

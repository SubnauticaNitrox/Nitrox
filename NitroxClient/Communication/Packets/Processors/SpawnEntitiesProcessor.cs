using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    class SpawnEntitiesProcessor : ClientPacketProcessor<SpawnEntities>
    {
        private PacketSender packetSender;
        private HashSet<String> alreadySpawnedGuids = new HashSet<String>();
        
        public SpawnEntitiesProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(SpawnEntities packet)
        {
            foreach(SpawnedEntity entity in packet.Entities)
            {
                if(!alreadySpawnedGuids.Contains(entity.Guid))
                {
                    if (entity.TechType != TechType.None)
                    {
                        GameObject gameObject = CraftData.InstantiateFromPrefab(entity.TechType);
                        gameObject.transform.position = entity.Position;
                        GuidHelper.SetNewGuid(gameObject, entity.Guid);
                        gameObject.SetActive(true);

                        Log.Debug("Received spawned entity: " + entity.Guid + " at " + entity.Position + " of type " + entity.TechType);

                        if (entity.SimulatingPlayerId.IsPresent() && entity.SimulatingPlayerId.Get() == packetSender.PlayerId)
                        {
                            Log.Debug("Simulating positioning of: " + entity.Guid);
                            EntityPositionBroadcaster.WatchEntity(entity.Guid, gameObject);
                        }

                        alreadySpawnedGuids.Add(entity.Guid);
                    }

                    alreadySpawnedGuids.Add(entity.Guid);
                }

                Multiplayer.Logic.SimulationOwnership.AddOwnedGuid(entity.Guid, entity.SimulatingPlayerId.Get());
            }
        }
    }
}

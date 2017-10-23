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
        HashSet<String> alreadySpawnedGuids = new HashSet<String>();
        
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

                        alreadySpawnedGuids.Add(entity.Guid);
                        Log.Info("Received spawned entity: " + entity.Guid + " at " + entity.Position + " of type " + entity.TechType);
                    }

                    alreadySpawnedGuids.Add(entity.Guid);
                }

                Multiplayer.Logic.SimulationOwnership.AddOwnedGuid(entity.Guid, entity.SimulatingPlayerId.Get());
            }
        }
    }
}

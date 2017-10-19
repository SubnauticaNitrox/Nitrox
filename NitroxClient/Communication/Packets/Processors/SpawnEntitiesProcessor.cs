using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.HUD;
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
                    GameObject techPrefab = CraftData.GetPrefabForTechType(entity.TechType);
                    GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(techPrefab, entity.Position, Quaternion.FromToRotation(Vector3.up, Vector3.up));
                    GuidHelper.SetNewGuid(gameObject, entity.Guid);
                    gameObject.SetActive(true);

                    alreadySpawnedGuids.Add(entity.Guid);
                    Log.Info("Received spawned entity: " + entity.Guid + " at " + entity.Position);
                }
            }
        }
    }
}

using NitroxClient.Communication;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private PacketSender packetSender;

        public Entities(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastTransforms(Dictionary<String, GameObject> gameObjectsByGuid)
        {
            EntityTransformUpdates update = new EntityTransformUpdates(packetSender.PlayerId);

            foreach(KeyValuePair<String, GameObject> gameObjectWithGuid in gameObjectsByGuid)
            {
                GameObject go = gameObjectWithGuid.Value;

                if (go != null)
                {
                    update.AddUpdate(gameObjectWithGuid.Key, gameObjectWithGuid.Value.transform.position, gameObjectWithGuid.Value.transform.rotation);
                }
            }

            packetSender.Send(update);
        }
    }
}

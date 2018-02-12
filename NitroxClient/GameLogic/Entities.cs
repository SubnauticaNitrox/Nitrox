using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;

        public Entities(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
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
    }
}

using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel.Logger;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FireDousedProcessor : ClientPacketProcessor<FireDoused>
    {
        private readonly IPacketSender packetSender;

        public FireDousedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        /// <summary>
        /// Finds and executes <see cref="Fire.Douse(float)"/>. If the fire is extinguished, it will pass a large float to trigger the private
        /// <see cref="Fire.Extinguish()"/> method.
        /// </summary>
        public override void Process(FireDoused packet)
        {
            Optional<GameObject> gameObject = NitroxEntity.GetObjectFrom(packet.Id);
            if (!gameObject.HasValue)
            {
                // cannot find that ID. try to douse something at the same location.

                Optional<GameObject> parentObject = NitroxEntity.GetObjectFrom(packet.ParentId);
                Validate.IsPresent(parentObject, $"Found neither fire {packet.Id} nor parent {packet.ParentId}");

                // look for closest fire
                Vector3 firePos = new Vector3(packet.xpos, packet.ypos, packet.zpos);
                float maxDist = float.MaxValue;

                foreach (Fire candidate in parentObject.Value.GetComponentsInChildren<Fire>())
                {
                    float candidateDistance = firePos.DistanceSqrXZ(candidate.transform.position);
                    if (candidateDistance < maxDist)
                    {
                        maxDist = candidateDistance;
                        gameObject = Optional.Of(candidate.gameObject);
                    }
                }

                Validate.IsPresent(gameObject, $"Did not find any fires in parent {packet.ParentId}");

                NitroxId replacedId = NitroxEntity.GetId(gameObject.Value);
                Log.Info($"FireDousedProcessor: Using substitute fire {replacedId} instead of nonexistant {packet.Id}");
            }

            using (packetSender.Suppress<FireDoused>())
            {
                gameObject.Value.GetComponent<Fire>().Douse(packet.DouseAmount);
            }
        }
    }
}

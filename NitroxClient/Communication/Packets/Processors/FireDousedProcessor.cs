using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
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
            Optional<GameObject> fireGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if (fireGameObject.IsPresent())
            {
                using (packetSender.Suppress<FireDoused>())
                {
                    fireGameObject.Get().RequireComponent<Fire>().Douse(packet.DouseAmount);
                }
            }
            else
            {
                Log.Warn("[FireDousedProcessor Could not find Fire! Guid: " + packet.Guid + "]");
            }
        }
    }
}

using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class FireDousedProcessor : IClientPacketProcessor<FireDoused>
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
        public Task Process(IPacketProcessContext context, FireDoused packet)
        {
            GameObject fireGameObject = NitroxEntity.RequireObjectFrom(packet.Id);

            using (PacketSuppressor<FireDoused>.Suppress())
            {
                fireGameObject.RequireComponent<Fire>().Douse(packet.DouseAmount);
            }

            return Task.CompletedTask;
        }
    }
}

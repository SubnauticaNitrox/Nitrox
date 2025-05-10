using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Networking.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class OpenableStateChangedProcessor : IClientPacketProcessor<OpenableStateChanged>
    {
        private readonly IPacketSender packetSender;

        public OpenableStateChangedProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public Task Process(IPacketProcessContext context, OpenableStateChanged packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            Openable openable = gameObject.RequireComponent<Openable>();

            using (PacketSuppressor<OpenableStateChanged>.Suppress())
            {
                openable.PlayOpenAnimation(packet.IsOpen, packet.Duration);
            }

            return Task.CompletedTask;
        }
    }
}

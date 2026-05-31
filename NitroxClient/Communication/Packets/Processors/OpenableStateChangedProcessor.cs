using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class OpenableStateChangedProcessor : IClientPacketProcessor<OpenableStateChanged>
{
    public Task Process(ClientProcessorContext context, OpenableStateChanged packet)
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

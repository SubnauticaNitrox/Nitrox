using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireDousedProcessor(IPacketSender packetSender) : IClientPacketProcessor<FireDoused>
{
    private readonly IPacketSender packetSender = packetSender;

    /// <summary>
    ///     Finds and executes <see cref="Fire.Douse(float)" />. If the fire is extinguished, it will pass a large float to
    ///     trigger the private
    ///     <see cref="Fire.Extinguish()" /> method.
    /// </summary>
    public Task Process(ClientProcessorContext context, FireDoused packet)
    {
        GameObject fireGameObject = NitroxEntity.RequireObjectFrom(packet.Id);

        using (PacketSuppressor<FireDoused>.Suppress())
        {
            fireGameObject.RequireComponent<Fire>().Douse(packet.DouseAmount);
        }
        return Task.CompletedTask;
    }
}

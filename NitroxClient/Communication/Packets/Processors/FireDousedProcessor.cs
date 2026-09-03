using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireDousedProcessor(Fires fires, Entities entities) : IClientPacketProcessor<FireDoused>
{
    private readonly Fires fires = fires;
    private readonly Entities entities = entities;

    /// <summary>
    ///     Finds and executes <see cref="Fire.Douse(float)" />. If the fire is extinguished, it will pass a large float to
    ///     trigger the private
    ///     <see cref="Fire.Extinguished()" /> method.
    /// </summary>
    public Task Process(ClientProcessorContext context, FireDoused packet)
    {
        GameObject fireGameObject = NitroxEntity.RequireObjectFrom(packet.Id);
        Fire fire = fireGameObject.RequireComponentInChildren<Fire>();

        float douseAmount = fire.livemixin.health - packet.Health;

        if (fires.WasDousedRecently(packet.Id))
        {
            // Both players are dousing the fire at the same time.
            // Use DouseAmount instead of Health so the effects stack.
            douseAmount = packet.DouseAmount;
        }

        if (!packet.IsExtinguished)
        {
            // Prevents a desync where the fire could extinguish for one player but not another
            douseAmount = Mathf.Max(douseAmount, 0.1f);
        }

        if (douseAmount > 0f)
        {
            using (PacketSuppressor<FireDoused>.Suppress())
            {
                fire.Douse(douseAmount);
            }
        }
        else
        {
            // Fire health went up
            fire.livemixin.health = packet.Health;
        }

        if (packet.IsExtinguished)
        {
            entities.RemoveEntity(packet.Id);
            fire.Extinguished();
        }

        return Task.CompletedTask;
    }
}

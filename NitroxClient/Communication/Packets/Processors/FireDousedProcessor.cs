using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FireDousedProcessor(Entities entities) : IClientPacketProcessor<FireDoused>
{
    private readonly Entities entities = entities;

    // Used to find the delta between consecutive FireDoused packets
    private readonly Dictionary<NitroxId, float> lastReceivedHealth = [];

    /// <summary>
    ///     Finds and executes <see cref="Fire.Douse(float)" />. If the fire is extinguished, it will invoke the
    ///     <see cref="Fire.Extinguished()" /> method.
    /// </summary>
    public Task Process(ClientProcessorContext context, FireDoused packet)
    {
        Optional<GameObject> fireGameObject = NitroxEntity.GetObjectFrom(packet.Id);
        if (!fireGameObject.HasValue)
        {
            Log.Warn($"Can't find fire entity with id {packet.Id}");
            return Task.CompletedTask;
        }

        Fire fire = fireGameObject.Value.GetComponent<Fire>();
        if (!fire)
        {
            Log.Error($"Fire object with id {packet.Id} missing component");
            return Task.CompletedTask;
        }

        float douseAmount = fire.livemixin.health - packet.Health;

        if (lastReceivedHealth.TryGetValue(packet.Id, out float lastReceived))
        {
            // Both players are dousing the fire at the same time.
            // Use a different calculation so the effects stack.
            douseAmount = Mathf.Min(douseAmount, lastReceived - packet.Health);
        }

        lastReceivedHealth[packet.Id] = packet.Health;

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
            lastReceivedHealth.Remove(packet.Id);
            fire.Extinguished();
        }

        return Task.CompletedTask;
    }
}

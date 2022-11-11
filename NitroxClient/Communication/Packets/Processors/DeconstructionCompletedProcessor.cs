using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class DeconstructionCompletedProcessor : ClientPacketProcessor<DeconstructionCompleted>
{
    private readonly GeometryRespawnManager geometryRespawnManager;
    private readonly IPacketSender packetSender;

    public DeconstructionCompletedProcessor(GeometryRespawnManager geometryRespawnManager, IPacketSender packetSender)
    {
        this.geometryRespawnManager = geometryRespawnManager;
        this.packetSender = packetSender;
    }
    public override void Process(DeconstructionCompleted packet)
    {
        GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
        if (deconstructing.TryGetComponent(out Constructable constructable))
        {
            constructable.constructedAmount = 0;
            using (packetSender.Suppress<DeconstructionCompleted>())
            {
                constructable.Deconstruct();
            }
        }
        else
        {
            UnityEngine.Object.Destroy(deconstructing);
        }
        geometryRespawnManager.NitroxIdsToIgnore.Add(packet.Id);
        Log.Debug($"[DeconstructionCompletedProcessor] added NitroxId to ignore list {packet.Id}");
    }
}

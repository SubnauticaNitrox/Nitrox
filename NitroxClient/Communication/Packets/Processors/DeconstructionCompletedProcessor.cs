using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class DeconstructionCompletedProcessor : ClientPacketProcessor<DeconstructionCompleted>
    {
        public override void Process(DeconstructionCompleted packet)
        {
            GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);
            UnityEngine.Object.Destroy(deconstructing);

            GeometryRespawnManager.NitroxIdsToIgnore.Add(packet.Id);
            Log.Debug($"[DeconstructionCompletedProcessor] added NitroxId to ignore list {packet.Id}");
        }
    }
}

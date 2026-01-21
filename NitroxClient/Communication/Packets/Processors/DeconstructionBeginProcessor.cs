using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DeconstructionBeginProcessor : IClientPacketProcessor<DeconstructionBegin>
{
    public Task Process(ClientProcessorContext context, DeconstructionBegin packet)
    {
        Log.Info($"Received deconstruction packet for id: {packet.Id}");

        GameObject deconstructing = NitroxEntity.RequireObjectFrom(packet.Id);

        Constructable constructable = deconstructing.GetComponent<Constructable>();
        BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();

        using (PacketSuppressor<DeconstructionBegin>.Suppress())
        {
            if (baseDeconstructable != null)
            {
                Add(TransientObjectType.LATEST_DECONSTRUCTED_BASE_PIECE_GUID, packet.Id);
                baseDeconstructable.Deconstruct();
            }
            else if (constructable != null)
            {
                constructable.SetState(false, false);
            }
        }
        return Task.CompletedTask;
    }
}

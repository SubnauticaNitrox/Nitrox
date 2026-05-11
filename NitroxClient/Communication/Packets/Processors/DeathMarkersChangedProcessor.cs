using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class DeathMarkersChangedProcessor(LocalPlayer localPlayer) : IClientPacketProcessor<DeathMarkersChanged>
{
    private readonly LocalPlayer localPlayer = localPlayer;

    public Task Process(ClientProcessorContext context, DeathMarkersChanged packet)
    {
        localPlayer.MarkDeathPointsWithBeacon = packet.MarkDeathPointsWithBeacon;
        return Task.CompletedTask;
    }
}

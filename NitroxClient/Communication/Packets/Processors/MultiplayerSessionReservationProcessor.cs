using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class MultiplayerSessionReservationProcessor(IMultiplayerSession multiplayerSession) : IClientPacketProcessor<MultiplayerSessionReservation>
{
    private readonly IMultiplayerSession multiplayerSession = multiplayerSession;

    public Task Process(ClientProcessorContext context, MultiplayerSessionReservation packet)
    {
        multiplayerSession.ProcessReservationResponsePacket(packet);
        return Task.CompletedTask;
    }
}
